using FluentAssertions;
using Moq;
using Partners.Api.Tests.Infrastructure;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models.Enums;
using Partners.Core.Results;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Partners.Api.Tests.Endpoints;

public class PartnerEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PartnerEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/partners");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithValidToken_Returns200()
    {
        var token = await _factory.GetValidTokenAsync();
        _factory.PartnerServiceMock
            .Setup(s => s.GetAllPartnersAsync())
            .ReturnsAsync([]);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WithValidToken_PartnerNotFound_Returns404()
    {
        var token = await _factory.GetValidTokenAsync();
        _factory.PartnerServiceMock
            .Setup(s => s.GetPartnerDetailsByIdAsync(999))
            .ReturnsAsync((PartnerDetailResponse?)null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/partners/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidToken_ValidRequest_Returns201()
    {
        var token = await _factory.GetValidTokenAsync();
        _factory.PartnerServiceMock
            .Setup(s => s.CreatePartnerAsync(It.IsAny<CreatePartnerRequest>()))
            .ReturnsAsync(PartnerServiceResult.Ok(1));

        var request = new CreatePartnerRequest
        {
            FirstName = "Ivan",
            LastName = "Horvat",
            PartnerNumber = "12345678901234567890",
            PartnerTypeId = PartnerType.Personal,
            CreateByUser = "user@wiener.hr",
            IsForeign = false,
            Gender = Gender.M
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/partners", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithValidToken_InvalidRequest_Returns400()
    {
        var token = await _factory.GetValidTokenAsync();

        var request = new CreatePartnerRequest
        {
            FirstName = "",
            LastName = "",
            PartnerNumber = "invalid"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/partners", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
