using FluentValidation;
using Partners.Core.DTOs.Requests;

namespace Partners.Core.Validators;

public class CreatePolicyRequestValidator : AbstractValidator<CreatePolicyRequest>
{
    public CreatePolicyRequestValidator()
    {
        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .Length(10, 15);

        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.PartnerId)
            .NotNull().WithMessage("PartnerId is required.");
    }
}
