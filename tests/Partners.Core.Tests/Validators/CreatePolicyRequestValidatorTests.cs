using FluentValidation.TestHelper;
using Partners.Core.DTOs.Requests;
using Partners.Core.Validators;

namespace Partners.Core.Tests.Validators;

public class CreatePolicyRequestValidatorTests
{
    private readonly CreatePolicyRequestValidator _validator = new();

    private static CreatePolicyRequest ValidRequest() => new()
    {
        PolicyNumber = "POL1234567",
        Amount = 100m,
        PartnerId = 1
    };

    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("SHORT")]
    [InlineData("TOOLONGPOLICYNUMBER1")]
    public async Task PolicyNumber_InvalidLength_FailsValidation(string policyNumber)
    {
        var req = ValidRequest();
        req.PolicyNumber = policyNumber;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.PolicyNumber);
    }

    [Fact]
    public async Task Amount_Null_FailsValidation()
    {
        var req = ValidRequest();
        req.Amount = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Amount_ZeroOrNegative_FailsValidation(decimal amount)
    {
        var req = ValidRequest();
        req.Amount = amount;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task PartnerId_Null_FailsValidation()
    {
        var req = ValidRequest();
        req.PartnerId = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.PartnerId);
    }
}
