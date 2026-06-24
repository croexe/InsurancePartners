using FluentAssertions;
using Moq;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Services;

namespace Partners.Core.Tests.Services;

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _policyRepoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();
    private readonly Mock<IPartnerNotifier> _notifierMock = new();
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _service = new PolicyService(_policyRepoMock.Object, _partnerRepoMock.Object, _notifierMock.Object);
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
            .Setup(r => r.GetPartnerByIdAsync(1))
            .ReturnsAsync((Partner?)null);

        var result = await _service.CreateAsync(ValidRequest());

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("1"));
        _policyRepoMock.Verify(r => r.CreateAsync(It.IsAny<Policy>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsOkWithPolicyResponse()
    {
        _partnerRepoMock
            .Setup(r => r.GetPartnerByIdAsync(1))
            .ReturnsAsync(new Partner { Id = 1 });

        _policyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Policy>()))
            .ReturnsAsync(10);

        _policyRepoMock
            .Setup(r => r.GetPolicySummaryByPartnerIdAsync(1))
            .ReturnsAsync(new PartnerPolicySummaryResponse(1, 1, 500m));

        _notifierMock
            .Setup(n => n.NotifyPartnerFlagChangedAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.Policy.Should().NotBeNull();
        result.Policy!.Id.Should().Be(10);
        result.Policy.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_NotifiesSignalR()
    {
        _partnerRepoMock
            .Setup(r => r.GetPartnerByIdAsync(1))
            .ReturnsAsync(new Partner { Id = 1 });

        _policyRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Policy>()))
            .ReturnsAsync(1);

        _policyRepoMock
            .Setup(r => r.GetPolicySummaryByPartnerIdAsync(1))
            .ReturnsAsync(new PartnerPolicySummaryResponse(1, 1, 500m));

        _notifierMock
            .Setup(n => n.NotifyPartnerFlagChangedAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(ValidRequest());

        _notifierMock.Verify(n => n.NotifyPartnerFlagChangedAsync(1, It.IsAny<bool>()), Times.Once);
    }
}
