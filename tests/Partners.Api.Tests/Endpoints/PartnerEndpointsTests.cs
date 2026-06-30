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
            .Setup(s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
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
            .Setup(s => s.GetPartnerDetailsByIdAsync(999, It.IsAny<CancellationToken>()))
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
            .Setup(s => s.CreatePartnerAsync(It.IsAny<CreatePartnerRequest>(), It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task Request_ExceedsGlobalRateLimit_Returns429()
    {
        using var lowGlobalFactory = new LowGlobalLimitFactory();
        var client = lowGlobalFactory.CreateClient();

        await client.GetAsync("/api/partners");              // 1 — prolazi (401, ali trosi permit)
        await client.GetAsync("/api/partners");              // 2 — prolazi (401)
        var third = await client.GetAsync("/api/partners");  // 3 — odbijen globalnim limiterom

        third.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GetAll_CachedAcrossRequests_CallsServiceOnce()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.PartnerServiceMock
            .Setup(s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PartnerListItemResponse>());

        var token = await factory.GetValidTokenAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.GetAsync("/api/partners");   // miss → servis pozvan, kesirano
        await client.GetAsync("/api/partners");   // hit → posluzeno iz kesa

        factory.PartnerServiceMock.Verify(
            s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePartner_InvalidatesCache_NextGetCallsServiceAgain()
    {
        using var factory = new CustomWebApplicationFactory();
        factory.PartnerServiceMock
            .Setup(s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PartnerListItemResponse>());
        factory.PartnerServiceMock
            .Setup(s => s.CreatePartnerAsync(It.IsAny<CreatePartnerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PartnerServiceResult.Ok(1));

        var token = await factory.GetValidTokenAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        await client.GetAsync("/api/partners");                 // 1 — servis pozvan, kesirano
        await client.PostAsJsonAsync("/api/partners", request); // invalidira tag "partners"
        await client.GetAsync("/api/partners");                 // 2 — kes prazan → servis opet pozvan

        factory.PartnerServiceMock.Verify(
            s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ConcurrentRequests_ExceedConcurrencyLimit_Returns429()
    {
        using var factory = new LowConcurrencyFactory();
        var gate = new TaskCompletionSource();
        factory.PartnerServiceMock
            .Setup(s => s.GetAllPartnersAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await gate.Task;
                return Enumerable.Empty<PartnerListItemResponse>();
            });

        var token = await factory.GetValidTokenAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var first = client.GetAsync("/api/partners");         // uzme jedinu dozvolu, blokira na gate
        await Task.Delay(300);                                // osiguraj da je prvi u obradi
        var second = await client.GetAsync("/api/partners");  // nema dozvole, red=0 → 429

        second.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        gate.SetResult();
        await first;
    }

    private sealed class LowGlobalLimitFactory : CustomWebApplicationFactory
    {
        protected override int GlobalRateLimitPermits => 2;
    }

    private sealed class LowConcurrencyFactory : CustomWebApplicationFactory
    {
        protected override int ConcurrencyPermitLimit => 1;
        protected override int ConcurrencyQueueLimit => 0;
    }
}
