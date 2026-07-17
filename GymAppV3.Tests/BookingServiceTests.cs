using FluentAssertions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Services;


namespace GymAppV3.Tests
{
    public class BookingServiceTests : TestBase
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        private BookingService CreateSut() => new(Context, new FixedClock(Now));

        // Seeds a category and returns it.
        private async Task<ClassCategory> SeedCategory(string name)
        {
            var category = new ClassCategory { Name = name };
            Context.ClassCategories.Add(category);
            await Context.SaveChangesAsync();
            return category;
        }

        // Seeds a member and returns it.
        private async Task<Member> SeedMember(string email = "m@gym.gr")
        {
            var member = new Member
            {
                UserId = "u1",
                Firstname = "Maria",
                Lastname = "Nikolaou",
                Email = email,
                BirthDate = new DateOnly(1990, 5, 20),
                HasMedicalConditions = false,
                Address = new Address
                {
                    Street = "Main St 1",
                    City = "Athens",
                    State = "Attica",
                    ZipCode = "10434",
                    Country = "Greece"
                }
            };
            Context.Members.Add(member);
            await Context.SaveChangesAsync();
            return member;
        }

        // Seeds an active membership for a member in a given category, with a balance.
        private async Task<Membership> SeedMembership(
            Guid memberId, Guid categoryId, int remaining = 8,
            DateTimeOffset? start = null, DateTimeOffset? end = null)
        {
            var package = new MembershipPackage
            {
                Name = "Pack",
                Price = 50m,
                DurationInDays = 30,
                SessionsIncluded = 8,
                ClassCategoryId = categoryId
            };
            Context.MembershipPackages.Add(package);
            await Context.SaveChangesAsync();

            var membership = new Membership
            {
                MemberId = memberId,
                MembershipPackageId = package.Id,
                PricePaid = 50m,
                StartDate = start ?? Now.AddDays(-1),
                EndDate = end ?? Now.AddDays(29),
                RemainingSessions = remaining,
                Status = MembershipStatus.Active,
                RowVersion = Array.Empty<byte>()
            };
            Context.Memberships.Add(membership);
            await Context.SaveChangesAsync();
            return membership;
        }

        // Seeds a future session in a given category, with a free seat.
        private async Task<ClassSession> SeedSession(
            Guid categoryId, DateTimeOffset? startsAt = null, int availableSeats = 5)
        {
            var building = new GymBuilding
            {
                Name = "Main",
                Address = new Address
                {
                    Street = "St 1",
                    City = "Athens",
                    State = "Attica",
                    ZipCode = "10434",
                    Country = "Greece"
                }
            };
            var trainer = new Trainer
            {
                UserId = "t1",
                Firstname = "Alex",
                Lastname = "Pap",
                Email = "t@gym.gr"
            };
            var room = new ClassRoom { ClassRoomName = "A", Capacity = 10, GymBuilding = building };
            Context.AddRange(building, trainer, room);
            await Context.SaveChangesAsync();

            var session = new ClassSession
            {
                Title = "Class",
                ClassCategoryId = categoryId,
                StartsAt = startsAt ?? Now.AddDays(2),
                EndsAt = (startsAt ?? Now.AddDays(2)).AddMinutes(60),
                DurationInMinutes = 60,
                Capacity = 10,
                AvailableSeats = availableSeats,
                TrainerId = trainer.Id,
                ClassRoomId = room.Id,
                RowVersion = Array.Empty<byte>()
            };
            Context.ClassSessions.Add(session);
            await Context.SaveChangesAsync();
            return session;
        }

        // --- Happy path: books and decrements both counters ------------------------

        [Fact]
        public async Task BookAsync_creates_booking_and_decrements_seat_and_credit()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            var membership = await SeedMembership(member.Id, category.Id, remaining: 8);
            var session = await SeedSession(category.Id, availableSeats: 5);
            var sut = CreateSut();

            var result = await sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            result.Status.Should().Be(nameof(BookingStatus.Confirmed));

            // Seat and credit both dropped by one.
            var updatedSession = await Context.ClassSessions.FindAsync(session.Id);
            updatedSession!.AvailableSeats.Should().Be(4);

            var updatedMembership = await Context.Memberships.FindAsync(membership.Id);
            updatedMembership!.RemainingSessions.Should().Be(7);
        }

        // --- Category matching: Yoga membership cannot book a Pilates session -------

        [Fact]
        public async Task BookAsync_rejects_when_membership_category_does_not_match()
        {
            var pilates = await SeedCategory("Pilates");
            var yoga = await SeedCategory("Yoga");
            var member = await SeedMember();
            await SeedMembership(member.Id, yoga.Id);              // member has YOGA only
            var session = await SeedSession(pilates.Id);           // session is PILATES
            var sut = CreateSut();

            var act = () => sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- No balance: membership with zero remaining sessions -------------------

        [Fact]
        public async Task BookAsync_rejects_when_no_remaining_sessions()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            await SeedMembership(member.Id, category.Id, remaining: 0);   // no credit
            var session = await SeedSession(category.Id);
            var sut = CreateSut();

            var act = () => sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Full session -----------------------------------------------------------

        [Fact]
        public async Task BookAsync_rejects_when_session_is_full()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            await SeedMembership(member.Id, category.Id);
            var session = await SeedSession(category.Id, availableSeats: 0);   // full
            var sut = CreateSut();

            var act = () => sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Session already started ------------------------------------------------

        [Fact]
        public async Task BookAsync_rejects_when_session_already_started()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            await SeedMembership(member.Id, category.Id);
            var session = await SeedSession(category.Id, startsAt: Now.AddHours(-1)); // past
            var sut = CreateSut();

            var act = () => sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Double booking ---------------------------------------------------------

        [Fact]
        public async Task BookAsync_rejects_duplicate_booking_for_same_session()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            await SeedMembership(member.Id, category.Id, remaining: 8);
            var session = await SeedSession(category.Id);
            var sut = CreateSut();

            await sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            var act = () => sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Cancel more than 24h ahead: seat freed AND credit returned ------------

        [Fact]
        public async Task CancelAsync_more_than_24h_returns_seat_and_credit()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            var membership = await SeedMembership(member.Id, category.Id, remaining: 8);
            // Session is 2 days away → cancelling now is well over 24h.
            var session = await SeedSession(category.Id, startsAt: Now.AddDays(2), availableSeats: 5);
            var sut = CreateSut();

            var booking = await sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));
            // After booking: seat 4, credit 7.

            await sut.CancelAsync(booking.Id);

            var s = await Context.ClassSessions.FindAsync(session.Id);
            var m = await Context.Memberships.FindAsync(membership.Id);
            s!.AvailableSeats.Should().Be(5);      // seat returned
            m!.RemainingSessions.Should().Be(8);   // credit returned
        }

        // --- Cancel within 24h: seat freed but credit forfeited --------------------

        [Fact]
        public async Task CancelAsync_within_24h_frees_seat_but_forfeits_credit()
        {
            var category = await SeedCategory("Pilates");
            var member = await SeedMember();
            var membership = await SeedMembership(member.Id, category.Id, remaining: 8);
            // Session is 10 hours away → within the 24h window.
            var session = await SeedSession(category.Id, startsAt: Now.AddHours(10), availableSeats: 5);
            var sut = CreateSut();

            var booking = await sut.BookAsync(new CreateBookingCommand(member.Id, session.Id));
            // After booking: seat 4, credit 7.

            await sut.CancelAsync(booking.Id);

            var s = await Context.ClassSessions.FindAsync(session.Id);
            var m = await Context.Memberships.FindAsync(membership.Id);
            s!.AvailableSeats.Should().Be(5);      // seat returned
            m!.RemainingSessions.Should().Be(7);   // credit NOT returned (forfeited)
        }

    }
}
