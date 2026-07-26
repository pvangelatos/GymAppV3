using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class PurchaseMembershipCommandValidator : AbstractValidator<PurchaseMembershipCommand>
{
    public PurchaseMembershipCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.MembershipPackageId).NotEmpty();
    }
}
