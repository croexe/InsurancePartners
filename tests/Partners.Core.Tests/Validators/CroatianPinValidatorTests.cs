using FluentAssertions;
using Partners.Core.Validators;

namespace Partners.Core.Tests.Validators;

public class CroatianPinValidatorTests
{
    [Theory]
    [InlineData("12345678903")]
    [InlineData("00000000001")]
    public void IsValid_ValidOib_ReturnsTrue(string oib)
    {
        CroatianPinValidator.IsValid(oib).Should().BeTrue();
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("00000000000")]
    public void IsValid_InvalidCheckDigit_ReturnsFalse(string oib)
    {
        CroatianPinValidator.IsValid(oib).Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void IsValid_WrongLength_ReturnsFalse(string oib)
    {
        CroatianPinValidator.IsValid(oib).Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890a")]
    [InlineData("abcdefghijk")]
    public void IsValid_NonDigitCharacters_ReturnsFalse(string oib)
    {
        CroatianPinValidator.IsValid(oib).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_NullOrEmpty_ReturnsFalse(string? oib)
    {
        CroatianPinValidator.IsValid(oib).Should().BeFalse();
    }
}
