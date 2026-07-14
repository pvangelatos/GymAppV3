
using FluentAssertions;
using GymAppV3.Core.DTOs.Payment;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Infrastructure.Services;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;

namespace GymAppV3.Tests;

public class PaymentServiceTests : TestBase
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
    private PaymentService CreateSut() => new(Context, new FixedClock(Now));
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
                Street = "St 1",
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

    // --- Recording -------------------------------------------------------------

    [Fact]
    public async Task RecordAsync_persists_payment_with_derived_net_and_vat()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        // 124 gross at 24% → 100 net + 24 VAT.
        var result = await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 124m, 0.24m, PaymentMethod.Card));

        result.Amount.Should().Be(124m);
        result.NetAmount.Should().Be(100m);
        result.VatAmount.Should().Be(24m);
        result.Status.Should().Be(nameof(PaymentStatus.Completed));
    }

    [Fact]
    public async Task RecordAsync_throws_NotFound_when_member_missing()
    {
        var sut = CreateSut();

        var act = () => sut.RecordAsync(new RecordPaymentRequest(
            Guid.NewGuid(), null, 100m, 0.24m, PaymentMethod.Cash));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- VAT split always reconciles ------------------------------------------

    [Fact]
    public async Task RecordAsync_net_plus_vat_always_equals_gross()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        // An amount that doesn't divide cleanly, to stress the rounding.
        var result = await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 99.99m, 0.24m, PaymentMethod.Cash));

        // The key invariant: net + vat must equal gross exactly.
        (result.NetAmount + result.VatAmount).Should().Be(result.Amount);
    }

    // --- Monthly report --------------------------------------------------------

    [Fact]
    public async Task GetMonthlyReportAsync_sums_only_the_given_month()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        // Two payments; we'll report on whatever month "now" falls in.
        await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 124m, 0.24m, PaymentMethod.Card));
        await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 62m, 0.24m, PaymentMethod.Cash));

        
        var report = await sut.GetMonthlyReportAsync(2026, 1);

        report.PaymentCount.Should().Be(2);
        report.TotalGross.Should().Be(186m);          // 124 + 62
        // Totals must reconcile.
        (report.TotalNet + report.TotalVat).Should().Be(report.TotalGross);
    }

    [Fact]
    public async Task GetMonthlyReportAsync_returns_zeros_for_empty_month()
    {
        await SeedMember();
        var sut = CreateSut();

        // A month far in the past with no payments.
        var report = await sut.GetMonthlyReportAsync(2000, 1);

        report.PaymentCount.Should().Be(0);
        report.TotalGross.Should().Be(0m);
        report.TotalNet.Should().Be(0m);
        report.TotalVat.Should().Be(0m);
    }

    // --- Member payment history -----------------------------------------------

    [Fact]
    public async Task GetByMemberAsync_returns_all_payments_of_member()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 100m, 0.24m, PaymentMethod.Card));
        await sut.RecordAsync(new RecordPaymentRequest(
            member.Id, null, 50m, 0.24m, PaymentMethod.Cash));

        var result = await sut.GetByMemberAsync(member.Id);

        result.Should().HaveCount(2);
    }
}
