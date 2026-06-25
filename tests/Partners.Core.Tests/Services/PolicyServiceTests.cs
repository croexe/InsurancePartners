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
            .Setup(r => r.FetchPartnerByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("1"));
        _policyRepoMock.Verify(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsOkWithPolicyResponse()
    {
        _partnerRepoMock
            .Setup(r => r.FetchPartnerByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Partner { Id = 1 });

        _policyRepoMock
            .Setup(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _policyRepoMock
            .Setup(r => r.FetchPolicySummaryByPartnerIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicySummary(1, 1, 500m));

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.Policy.Should().NotBeNull();
        result.Policy!.Id.Should().Be(10);
        result.Policy.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task CreateAsync_PartnerOverThreshold_ReturnsResultFlagged()
    {
        _partnerRepoMock
            .Setup(r => r.FetchPartnerByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Partner { Id = 1 });

        _policyRepoMock
            .Setup(r => r.InsertPolicyAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _policyRepoMock
            .Setup(r => r.FetchPolicySummaryByPartnerIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicySummary(1, 6, 9000m));

        var result = await _service.CreatePolicyAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.IsFlagged.Should().BeTrue();
    }
}
