using FluentValidation;
using GymAppV3.Core.Queries.Payments;

namespace GymAppV3.Core.Validators;

public class GetMonthlyFinancialReportQueryValidator : AbstractValidator<GetMonthlyFinancialReportQuery>
{
    public GetMonthlyFinancialReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
