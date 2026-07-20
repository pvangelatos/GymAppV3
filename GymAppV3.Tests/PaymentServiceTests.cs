
using FluentAssertions;
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Infrastructure.Handlers.Queries.Payments;
using GymAppV3.Infrastructure.Services;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.Queries.Payments;

namespace GymAppV3.Tests;

public class PaymentServiceTests : TestBase
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly IVatRateProvider _vatRates = new MockVatRateProvider();

    private PaymentService CreateSut() => new(Context, new FixedClock(Now), _vatRates);
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

        // 113 gross at 13% → 100 net + 13 VAT.
        var result = await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 113m, PaymentMethod.Card));

        result.Amount.Should().Be(113m);
        result.NetAmount.Should().Be(100m);
        result.VatAmount.Should().Be(13m);
        result.Status.Should().Be(nameof(PaymentStatus.Completed));
    }

    [Fact]
    public async Task RecordAsync_throws_NotFound_when_member_missing()
    {
        var sut = CreateSut();

        var act = () => sut.RecordAsync(new RecordPaymentCommand(
            Guid.NewGuid(), null, 100m, PaymentMethod.Cash));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- VAT split always reconciles ------------------------------------------

    [Fact]
    public async Task RecordAsync_net_plus_vat_always_equals_gross()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        // An amount that doesn't divide cleanly, to stress the rounding.
        var result = await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 99.99m, PaymentMethod.Cash));

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
        await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 124m, PaymentMethod.Card));
        await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 62m, PaymentMethod.Cash));

        var reportHandler = new GetMonthlyFinancialReportQueryHandler(Context);
        var report = await reportHandler.HandleAsync(new GetMonthlyFinancialReportQuery(2026, 1));

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
        var reportHandler = new GetMonthlyFinancialReportQueryHandler(Context);
        var report = await reportHandler.HandleAsync(new GetMonthlyFinancialReportQuery(2000, 1));

        report.PaymentCount.Should().Be(0);
        report.TotalGross.Should().Be(0m);
        report.TotalNet.Should().Be(0m);
        report.TotalVat.Should().Be(0m);
    }

    // --- Member payment history -----------------------------------------------

    [Fact]
    public async Task GetPaymentsByMemberAsync_returns_all_payments_of_member()
    {
        var member = await SeedMember();
        var sut = CreateSut();

        await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 100m, PaymentMethod.Card));
        await sut.RecordAsync(new RecordPaymentCommand(
            member.Id, null, 50m, PaymentMethod.Cash));

        var historyHandler = new GetPaymentsByMemberQueryHandler(Context);
        var result = await historyHandler.HandleAsync(new GetPaymentsByMemberQuery(member.Id));

        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Mock implementation of IVatRateProvider with fixed 24% Greece rate
    /// </summary>
    internal class MockVatRateProvider : IVatRateProvider
    {
        public decimal GetVatRate(VatCategory category) => category == VatCategory.Services ? 0.13m : 0.24m;
    }
}
