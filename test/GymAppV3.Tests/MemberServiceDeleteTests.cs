using FluentAssertions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GymAppV3.Tests;

public class MemberServiceDeleteTests : TestBase
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    // UserManager is not used by DeleteAsync, so null is safe here.
    private MemberService CreateSut() =>
        new(Context, UserContext, new FixedClock(Now), null!, new ConfigurationBuilder().Build());

    private static Member NewMember() => new()
    {
        UserId = "u1",
        Firstname = "Maria",
        Lastname = "Nikolaou",
        Email = "del@gym.gr",
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

    private static ClassSession NewSession(DateTimeOffset startsAt, int availableSeats) => new()
    {
        Title = "Pilates",
        StartsAt = startsAt,
        EndsAt = startsAt.AddHours(1),
        DurationInMinutes = 60,
        Capacity = 10,
        AvailableSeats = availableSeats,
        TrainerId = Guid.NewGuid(),
        ClassRoomId = Guid.NewGuid(),
        ClassCategoryId = Guid.NewGuid()
    };

    private static Booking NewBooking(Member member, ClassSession session) => new()
    {
        Member = member,
        MemberId = member.Id,
        ClassSession = session,
        ClassSessionId = session.Id,
        Status = BookingStatus.Confirmed,
        BookedAt = Now.AddDays(-5)
    };

    [Fact]
    public async Task DeleteAsync_MemberWithBookings_ReleasesFutureSeatsAndKeepsPastBookings()
    {
        var member = NewMember();
        var past = NewSession(Now.AddDays(-2), availableSeats: 9);
        var future = NewSession(Now.AddDays(2), availableSeats: 9);
        var pastBooking = NewBooking(member, past);
        var futureBooking = NewBooking(member, future);

        Context.AddRange(member, past, future, pastBooking, futureBooking);
        await Context.SaveChangesAsync();

        UserContext.As("admin1", RoleConstants.Admin);

        await CreateSut().DeleteAsync(new DeleteMemberCommand(member.Id));

        futureBooking.Status.Should().Be(BookingStatus.Cancelled);
        future.AvailableSeats.Should().Be(10);
        pastBooking.Status.Should().Be(BookingStatus.Confirmed);

        var deleted = await Context.Members.IgnoreQueryFilters().SingleAsync(m => m.Id == member.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }
}
