using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.MembershipId)
            .NotEqual(Guid.Empty)
            .When(x => x.MembershipId.HasValue);
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.Method).IsInEnum();
    }
}
