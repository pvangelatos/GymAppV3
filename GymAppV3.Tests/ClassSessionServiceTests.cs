using FluentAssertions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Infrastructure.Services;


namespace GymAppV3.Tests
{
    public class ClassSessionServiceTests : TestBase
    {
        // A fixed "now" so time-based rules are deterministic. The service uses the
        // injected clock, so tests are not affected by the real wall-clock time.
        private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        private ClassSessionService CreateSut() =>
            new(Context, new FixedClock(Now));

        // Seeds a trainer and a room (capacity 8) and returns their ids, since every
        // schedule call needs both to exist.
        private async Task<(Guid trainerId, Guid roomId, Guid categoryId)> SeedTrainerAndRoom(int roomCapacity = 8)
        {
            var building = new GymBuilding
            {
                Name = "Main",
                Address = new Address
                {
                    Street = "Main St 1",
                    City = "Athens",
                    State = "Attica",
                    ZipCode = "10434",
                    Country = "Greece"
                }
            };
            var trainer = new Trainer
            {
                UserId = "u1",
                Firstname = "Alex",
                Lastname = "Papadopoulos",
                Email = "alex@gym.gr"
            };
            var room = new ClassRoom
            {
                ClassRoomName = "Studio A",
                Capacity = roomCapacity,
                GymBuilding = building
            };

            var category = new ClassCategory
            {
                Name = $"{nameof(ClassCategory)}",
            };

            Context.GymBuildings.Add(building);
            Context.Trainers.Add(trainer);
            Context.ClassRooms.Add(room);
            Context.ClassCategories.Add(category);
            await Context.SaveChangesAsync();

            return (trainer.Id, room.Id, category.Id);
        }

        private static ScheduleClassSessionCommand Request(
            Guid trainerId, Guid roomId, Guid categoryId,
            DateTimeOffset? startsAt = null, int capacity = 6, int duration = 60) =>
            new(
                "Morning Yoga",
                categoryId,
                startsAt ?? Now.AddDays(1),
                duration,
                capacity,
                trainerId,
                roomId);

        // --- Happy path ---------------------------------------------------------

        [Fact]
        public async Task ScheduleAsync_creates_session_when_all_rules_pass()
        {
            var (trainerId, roomId, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            var result = await sut.ScheduleAsync(Request(trainerId, roomId, categoryId));

            result.Id.Should().NotBeEmpty();
            result.Title.Should().Be("Morning Yoga");
            // AvailableSeats starts equal to Capacity — no bookings yet.
            result.AvailableSeats.Should().Be(result.Capacity);
            result.TrainerName.Should().Be("Alex Papadopoulos");
            result.ClassRoomName.Should().Be("Studio A");
        }

        // --- Rule 2: no scheduling in the past ---------------------------------

        [Fact]
        public async Task ScheduleAsync_rejects_a_session_in_the_past()
        {
            var (trainerId, roomId, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            var act = () => sut.ScheduleAsync(
                Request(trainerId, roomId, categoryId, startsAt: Now.AddHours(-1)));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Rule 3 & 4: trainer / room must exist -----------------------------

        [Fact]
        public async Task ScheduleAsync_throws_NotFound_when_trainer_missing()
        {
            var (_, roomId, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            var act = () => sut.ScheduleAsync(Request(Guid.NewGuid(), roomId, categoryId));

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ScheduleAsync_throws_NotFound_when_room_missing()
        {
            var (trainerId, _, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            var act = () => sut.ScheduleAsync(Request(trainerId, Guid.NewGuid(), categoryId));

            await act.Should().ThrowAsync<NotFoundException>();
        }

        // --- Rule 1: capacity cannot exceed room capacity ----------------------

        [Fact]
        public async Task ScheduleAsync_rejects_capacity_over_room_capacity()
        {
            var (trainerId, roomId, categoryId) = await SeedTrainerAndRoom(roomCapacity: 8);
            var sut = CreateSut();

            // Room holds 8; asking for 12 must fail.
            var act = () => sut.ScheduleAsync(Request(trainerId, roomId, categoryId, capacity: 12));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        // --- Rule 5: no overlapping session in the same room -------------------

        [Fact]
        public async Task ScheduleAsync_rejects_overlapping_session_in_same_room()
        {
            var (trainerId, roomId, categoryId ) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            // First session: tomorrow 09:00, 60 min → ends 10:00.
            var start = Now.AddDays(1);
            await sut.ScheduleAsync(Request(trainerId, roomId, categoryId, startsAt: start, duration: 60));

            // Second session: overlaps (starts 09:30, while first runs until 10:00).
            var act = () => sut.ScheduleAsync(
                Request(trainerId, roomId, categoryId, startsAt: start.AddMinutes(30), duration: 60));

            await act.Should().ThrowAsync<BusinessRuleException>();
        }

        [Fact]
        public async Task ScheduleAsync_allows_back_to_back_sessions_in_same_room()
        {
            var (trainerId, roomId, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            var start = Now.AddDays(1);
            await sut.ScheduleAsync(Request(trainerId, roomId, categoryId, startsAt: start, duration: 60));

            // Starts exactly when the first ends — touching, not overlapping. Allowed.
            var act = () => sut.ScheduleAsync(
                Request(trainerId, roomId, categoryId, startsAt: start.AddMinutes(60), duration: 60));

            await act.Should().NotThrowAsync();
        }

        // --- GetUpcoming --------------------------------------------------------

        [Fact]
        public async Task GetUpcomingAsync_returns_only_future_sessions_ordered()
        {
            var (trainerId, roomId, categoryId) = await SeedTrainerAndRoom();
            var sut = CreateSut();

            await sut.ScheduleAsync(Request(trainerId, roomId, categoryId, startsAt: Now.AddDays(2)));
            await sut.ScheduleAsync(Request(trainerId, roomId, categoryId, startsAt: Now.AddDays(1)));

            var result = await sut.GetUpcomingAsync(new GetUpcomingClassSessionsQuery());

            result.Should().HaveCount(2);
            // Ordered by start time ascending.
            result[0].StartsAt.Should().BeBefore(result[1].StartsAt);
        }
    }
}
