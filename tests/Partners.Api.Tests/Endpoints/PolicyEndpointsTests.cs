using FluentAssertions;
using Moq;
using Partners.Api.Tests.Infrastructure;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Results;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Partners.Api.Tests.Endpoints;

public class PolicyEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PolicyEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/policies", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithValidToken_ValidRequest_Returns201()
    {
        var token = await _factory.GetValidTokenAsync();
        _factory.PolicyServiceMock
            .Setup(s => s.CreatePolicyAsync(It.IsAny<CreatePolicyRequest>()))
            .ReturnsAsync(PolicyServiceResult.Ok(new PolicyResponse
            {
                Id = 1,
                PolicyNumber = "POL1234567",
                Amount = 500m,
                PartnerId = 1
            }, isFlagged: true));

        var request = new CreatePolicyRequest
        {
            PolicyNumber = "POL1234567",
            Amount = 500m,
            PartnerId = 1
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/policies", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.PartnerNotifierMock.Verify(
            n => n.NotifyPartnerFlagChangedAsync(1, true), Times.Once);
    }

    [Fact]
    public async Task Create_NotifierThrows_StillReturns201()
    {
        var token = await _factory.GetValidTokenAsync();
        _factory.PolicyServiceMock
            .Setup(s => s.CreatePolicyAsync(It.IsAny<CreatePolicyRequest>()))
            .ReturnsAsync(PolicyServiceResult.Ok(new PolicyResponse
            {
                Id = 2,
                PolicyNumber = "POL7654321",
                Amount = 500m,
                PartnerId = 99
            }, isFlagged: false));

        // Specifican partnerId (99) da setup ne kontaminira ostale testove koji koriste partnerId 1.
        _factory.PartnerNotifierMock
            .Setup(n => n.NotifyPartnerFlagChangedAsync(99, It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SignalR hub unavailable"));

        var request = new CreatePolicyRequest
        {
            PolicyNumber = "POL7654321",
            Amount = 500m,
            PartnerId = 99
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/policies", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithValidToken_InvalidRequest_Returns400()
    {
        var token = await _factory.GetValidTokenAsync();

        var request = new CreatePolicyRequest
        {
            PolicyNumber = "",
            Amount = -1m,
            PartnerId = null
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/policies", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
