using FluentValidation;
using Partners.Core.DTOs.Requests;

namespace Partners.Core.Validators;

public class CreatePartnerRequestValidator : AbstractValidator<CreatePartnerRequest>
{
    public CreatePartnerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(2, 255)
            .Matches(@"^[a-zA-Z0-9À-ſ ]+$").WithMessage("FirstName must be alphanumeric.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(2, 255)
            .Matches(@"^[a-zA-Z0-9À-ſ ]+$").WithMessage("LastName must be alphanumeric.");

        RuleFor(x => x.Address)
            .Matches(@"^[a-zA-Z0-9À-ſ ,.\-]+$").WithMessage("Address must be alphanumeric.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PartnerNumber)
            .NotEmpty()
            .Matches(@"^\d{20}$").WithMessage("PartnerNumber must be exactly 20 digits.");

        RuleFor(x => x.CroatianPIN)
            .Matches(@"^\d{11}$").WithMessage("CroatianPIN must be exactly 11 digits.")
            .Must(CroatianPinValidator.IsValid).WithMessage("CroatianPIN (OIB) is not valid.")
            .When(x => !string.IsNullOrWhiteSpace(x.CroatianPIN));

        RuleFor(x => x.PartnerTypeId)
            .NotNull().WithMessage("PartnerTypeId is required.");

        RuleFor(x => x.CreateByUser)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(x => x.IsForeign)
            .NotNull().WithMessage("IsForeign is required.");

        RuleFor(x => x.ExternalCode)
            .Length(10, 20)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalCode));

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Gender is required.");
    }
}
