using FluentAssertions;
using Moq;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.Models;
using Partners.Core.Services;

namespace Partners.Core.Tests.Services;

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _service = new PolicyService(_policyRepoMock.Object, _partnerRepoMock.Object);
    }

    private static CreatePolicyRequest ValidRequest() => new()
    {
        PolicyNumber = "POL1234567",
        Amount = 500m,
        PartnerId = 1
    };

    [Fact]
    public async Task CreateAsync_PartnerDoesNotExist_ReturnsFail()
    {
        _partnerRepoMock
            .Setup(r => r.PartnerExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("1"));
        _policyRepoMock.Verify(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsOkWithPolicyResponse()
    {
        _partnerRepoMock
            .Setup(r => r.PartnerExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _policyRepoMock
            .Setup(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _policyRepoMock
            .Setup(r => r.FetchPolicySummaryByPartnerIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicySummary(1, 1, 500m));

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Policy.Id.Should().Be(10);
        result.Value!.Policy.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task CreateAsync_PartnerOverThreshold_ReturnsResultFlagged()
    {
        _partnerRepoMock
            .Setup(r => r.PartnerExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _policyRepoMock
            .Setup(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _policyRepoMock
            .Setup(r => r.FetchPolicySummaryByPartnerIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicySummary(1, 6, 9000m));

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.Value!.IsFlagged.Should().BeTrue();
    }
}
