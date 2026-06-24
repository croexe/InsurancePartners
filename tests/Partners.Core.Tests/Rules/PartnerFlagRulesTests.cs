using FluentAssertions;
using Partners.Core.Models.Rules.Partner;

namespace Partners.Core.Tests.Rules;

public class PartnerFlagRulesTests
{
    [Theory]
    [InlineData(6, 0)]
    [InlineData(0, 5001)]
    [InlineData(6, 5001)]
    public void IsFlagged_AboveThreshold_ReturnsTrue(int policyCount, decimal totalAmount)
    {
        PartnerFlagRules.IsFlagged(policyCount, totalAmount).Should().BeTrue();
    }

    [Theory]
    [InlineData(5, 5000)]
    [InlineData(0, 0)]
    [InlineData(5, 0)]
    [InlineData(0, 5000)]
    public void IsFlagged_AtOrBelowThreshold_ReturnsFalse(int policyCount, decimal totalAmount)
    {
        PartnerFlagRules.IsFlagged(policyCount, totalAmount).Should().BeFalse();
    }
}
