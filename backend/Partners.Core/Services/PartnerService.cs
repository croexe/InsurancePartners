using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
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

    public async Task<IEnumerable<PartnerListItemResponse>> GetAllPartnersAsync(CancellationToken cancellationToken = default)
    {
        var partners = await _partnerRepository.FetchAllPartnersAsync(cancellationToken);

        return partners.Select(item => item.ToListItemResponse());
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
        if (!string.IsNullOrWhiteSpace(request.ExternalCode))
        {
            var exists = await _partnerRepository.ExternalCodeExistsAsync(request.ExternalCode, cancellationToken);
            if (exists)
            {
                return Result<int>.Fail($"ExternalCode '{request.ExternalCode}' is already in use.");
            }
        }

        var partner = request.ToPartner();
        var newId = await _partnerRepository.InsertPartnerAsync(partner, cancellationToken);

        return Result<int>.Ok(newId);
    }
}
