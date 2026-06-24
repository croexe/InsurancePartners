using FluentAssertions;
using Moq;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.Models;
using Partners.Core.Models.Enums;
using Partners.Core.Services;

namespace Partners.Core.Tests.Services;

public class PartnerServiceTests
{
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();
    private readonly Mock<IPolicyRepository> _policyRepoMock = new();
    private readonly PartnerService _service;

    public PartnerServiceTests()
    {
        _service = new PartnerService(_partnerRepoMock.Object, _policyRepoMock.Object);
    }

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
    public async Task CreateAsync_ExternalCodeAlreadyExists_ReturnsFail()
    {
        var request = ValidRequest();
        request.ExternalCode = "EXT123456789";

        _partnerRepoMock
            .Setup(r => r.ExternalCodeExistsAsync("EXT123456789"))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("EXT123456789"));
        _partnerRepoMock.Verify(r => r.CreateAsync(It.IsAny<Partner>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsOkWithNewId()
    {
        _partnerRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Partner>()))
            .ReturnsAsync(42);

        var result = await _service.CreateAsync(ValidRequest());

        result.Success.Should().BeTrue();
        result.PartnerId.Should().Be(42);
    }

    [Fact]
    public async Task CreateAsync_ValidRequestWithExternalCode_CodeFree_ReturnsOk()
    {
        var request = ValidRequest();
        request.ExternalCode = "EXT123456789";

        _partnerRepoMock
            .Setup(r => r.ExternalCodeExistsAsync("EXT123456789"))
            .ReturnsAsync(false);

        _partnerRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Partner>()))
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_PartnerDoesNotExist_ReturnsNull()
    {
        _partnerRepoMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Partner?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }
}
