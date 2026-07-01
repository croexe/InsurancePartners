using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Exceptions;
using Partners.Core.Mapping;
using Partners.Core.Results;

namespace Partners.Core.Services;

public class PartnerService : IPartnerService
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly IPolicyRepository _policyRepository;

    public PartnerService(IPartnerRepository partnerRepository, IPolicyRepository policyRepository)
    {
        _partnerRepository = partnerRepository;
        _policyRepository = policyRepository;
    }

    public async Task<PagedResponse<PartnerListItemResponse>> GetPartnersPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var offset = (page - 1) * pageSize;
        var (items, totalCount) = await _partnerRepository.FetchPartnersPageAsync(offset, pageSize, cancellationToken);

        return new PagedResponse<PartnerListItemResponse>
        {
            Items = items.Select(item => item.ToListItemResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PartnerDetailResponse?> GetPartnerDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partner = await _partnerRepository.FetchPartnerByIdAsync(id, cancellationToken);
        if (partner is null)
        {
            return null;
        }

        var policies = await _policyRepository.FetchAllPoliciesByPartnerIdAsync(id, cancellationToken);

        return partner.ToDetailResponse(policies);
    }

    public async Task<Result<int>> CreatePartnerAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default)
    {
        var partner = request.ToPartner();

        try
        {
            var newId = await _partnerRepository.InsertPartnerAsync(partner, cancellationToken);
            return Result<int>.Ok(newId);
        }
        catch (DuplicateExternalCodeException)
        {
            return Result<int>.Fail($"ExternalCode '{request.ExternalCode}' is already in use.");
        }
    }
}
