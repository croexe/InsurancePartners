using FluentValidation.TestHelper;
using Partners.Core.DTOs.Requests;
using Partners.Core.Models.Enums;
using Partners.Core.Validators;

namespace Partners.Core.Tests.Validators;

public class CreatePartnerRequestValidatorTests
{
    private readonly CreatePartnerRequestValidator _validator = new();

    private static CreatePartnerRequest ValidRequest() => new()
    {
        FirstName = "Ivan",
        LastName = "Horvat",
        PartnerNumber = "12345678901234567890",
        PartnerTypeId = PartnerType.Personal,
        CreateByUser = "user@wiener.hr",
        IsForeign = false,
        Gender = Gender.M
    };

    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public async Task FirstName_TooShortOrEmpty_FailsValidation(string firstName)
    {
        var req = ValidRequest();
        req.FirstName = firstName;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task FirstName_InvalidCharacters_FailsValidation()
    {
        var req = ValidRequest();
        req.FirstName = "Ivan@#$";
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task PartnerNumber_Not20Digits_FailsValidation()
    {
        var req = ValidRequest();
        req.PartnerNumber = "12345";
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.PartnerNumber);
    }

    [Fact]
    public async Task CroatianPIN_InvalidOib_FailsValidation()
    {
        var req = ValidRequest();
        req.CroatianPIN = "12345678900";
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.CroatianPIN);
    }

    [Fact]
    public async Task CroatianPIN_Null_PassesValidation()
    {
        var req = ValidRequest();
        req.CroatianPIN = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldNotHaveValidationErrorFor(x => x.CroatianPIN);
    }

    [Fact]
    public async Task CreateByUser_NotEmail_FailsValidation()
    {
        var req = ValidRequest();
        req.CreateByUser = "notanemail";
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.CreateByUser);
    }

    [Fact]
    public async Task PartnerTypeId_Null_FailsValidation()
    {
        var req = ValidRequest();
        req.PartnerTypeId = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.PartnerTypeId);
    }

    [Fact]
    public async Task IsForeign_Null_FailsValidation()
    {
        var req = ValidRequest();
        req.IsForeign = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.IsForeign);
    }

    [Fact]
    public async Task Gender_Null_FailsValidation()
    {
        var req = ValidRequest();
        req.Gender = null;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("SHORT")]
    [InlineData("TOOLONGEXTERNALCODE123")]
    public async Task ExternalCode_InvalidLength_FailsValidation(string externalCode)
    {
        var req = ValidRequest();
        req.ExternalCode = externalCode;
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.ExternalCode);
    }
}
