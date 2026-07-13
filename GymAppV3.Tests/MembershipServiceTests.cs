using FluentAssertions;
using GymAppV3.Core.DTOs.Membership;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Services;
using Xunit;

namespace GymAppV3.Tests;

public class MembershipServiceTests : TestBase
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private MembershipService CreateSut() => new(Context, new FixedClock(Now));

    // Seeds a member and returns its id.
    private async Task<Guid> SeedMember(string email = "m@gym.gr")
    {
        var member = new Member
        {
            UserId = "u1",
            Firstname = "Maria",
            Lastname = "Nikolaou",
            Email = email,
            BirthDate = new DateOnly(1990, 5, 20),   // ← required τώρα
            HasMedicalConditions = false,
            Address = new Address                      // ← required τώρα
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
        return member.Id;
    }

    // Seeds a package and returns its id.
    private async Task<Guid> SeedPackage(
        string name = "Pilates", decimal price = 50m, int days = 30, int sessions = 8)
    {
        var categoryId = await SeedCategory(name);      // Category with the same name of the package
        var package = new MembershipPackage
        {
            Name = name,
            Price = price,
            DurationInDays = days,
            SessionsIncluded = sessions,
            ClassCategoryId = categoryId
        };
        Context.MembershipPackages.Add(package);
        await Context.SaveChangesAsync();
        return package.Id;
    }

    // --- First purchase: starts now, derives everything from the package --------

    [Fact]
    public async Task PurchaseAsync_first_purchase_starts_now_and_derives_from_package()
    {
        var memberId = await SeedMember();
        var packageId = await SeedPackage(price: 50m, days: 30, sessions: 8);
        var sut = CreateSut();

        var result = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, packageId));

        result.StartDate.Should().Be(Now);
        result.EndDate.Should().Be(Now.AddDays(30));
        result.RemainingSessions.Should().Be(8);
        result.PricePaid.Should().Be(50m);
        result.Status.Should().Be(nameof(MembershipStatus.Active));
    }

    // --- Price snapshot: later price change does not affect an existing membership ---

    [Fact]
    public async Task PurchaseAsync_freezes_the_price_at_purchase_time()
    {
        var memberId = await SeedMember();
        var packageId = await SeedPackage(price: 50m);
        var sut = CreateSut();

        var membership = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, packageId));

        // Change the package price afterwards.
        var package = await Context.MembershipPackages.FindAsync(packageId);
        package!.Price = 80m;
        await Context.SaveChangesAsync();

        // The membership still remembers what was actually paid.
        var reloaded = await sut.GetByIdAsync(membership.Id);
        reloaded!.PricePaid.Should().Be(50m);
    }

    // --- Renewal stacking: same package stacks onto the previous end date -------

    [Fact]
    public async Task PurchaseAsync_stacks_renewal_of_same_package()
    {
        var memberId = await SeedMember();
        var packageId = await SeedPackage(days: 30);
        var sut = CreateSut();

        // First Pilates: 15/1 → 14/2.
        var first = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, packageId));

        // Second Pilates (renewal): must start when the first ends, not now.
        var second = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, packageId));

        second.StartDate.Should().Be(first.EndDate);
        second.EndDate.Should().Be(first.EndDate.AddDays(30));
    }

    // --- Different packages run in parallel from now ----------------------------

    [Fact]
    public async Task PurchaseAsync_different_packages_run_in_parallel()
    {
        var memberId = await SeedMember();
        var pilatesId = await SeedPackage(name: "Pilates", days: 30);
        var yogaId = await SeedPackage(name: "Yoga", days: 30);
        var sut = CreateSut();

        var pilates = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, pilatesId));
        var yoga = await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, yogaId));

        // Yoga ignores Pilates — both start now.
        yoga.StartDate.Should().Be(Now);
        pilates.StartDate.Should().Be(Now);
    }

    // --- Existence checks -------------------------------------------------------

    [Fact]
    public async Task PurchaseAsync_throws_NotFound_when_member_missing()
    {
        var packageId = await SeedPackage();
        var sut = CreateSut();

        var act = () => sut.PurchaseAsync(new PurchaseMembershipRequest(Guid.NewGuid(), packageId));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PurchaseAsync_throws_NotFound_when_package_missing()
    {
        var memberId = await SeedMember();
        var sut = CreateSut();

        var act = () => sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetByMember ------------------------------------------------------------

    [Fact]
    public async Task GetByMemberAsync_returns_all_memberships_of_the_member()
    {
        var memberId = await SeedMember();
        var pilatesId = await SeedPackage(name: "Pilates");
        var yogaId = await SeedPackage(name: "Yoga");
        var sut = CreateSut();

        await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, pilatesId));
        await sut.PurchaseAsync(new PurchaseMembershipRequest(memberId, yogaId));

        var result = await sut.GetByMemberAsync(memberId);

        result.Should().HaveCount(2);
    }

    // Seeds a class category and returns its id. Memberships now requires one.
    private async Task<Guid> SeedCategory(string name = "Pilates Reformer")
    {
        var category = new ClassCategory { Name = name };
        Context.ClassCategories.Add(category);
        await Context.SaveChangesAsync();
        return category.Id;
    }
}
