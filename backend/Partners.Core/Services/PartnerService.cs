using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Models.Rules.Partner;
using Partners.Core.Presentation;
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

        return partners.Select(item => new PartnerListItemResponse
        {
            Id = item.Partner.Id,
            FullName = $"{item.Partner.FirstName} {item.Partner.LastName}",
            PartnerNumber = item.Partner.PartnerNumber,
            CroatianPIN = item.Partner.CroatianPIN,
            PartnerTypeName = item.Partner.PartnerTypeId.ToDisplayName(),
            CreatedAtUtc = item.Partner.CreatedAtUtc,
            IsForeign = item.Partner.IsForeign,
            Gender = item.Partner.Gender.ToString(),
            IsFlagged = IsFlagged(item.PolicyCount, item.TotalAmount)
        });
    }

    public async Task<PartnerDetailResponse?> GetPartnerDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partner = await _partnerRepository.FetchPartnerByIdAsync(id, cancellationToken);
        if (partner is null)
        {
            return null;
        }

        var policies = await _policyRepository.FetchAllPoliciesByPartnerIdAsync(id, cancellationToken);
        var policyResponses = policies
            .Select(p => new PolicyResponse
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                Amount = p.Amount,
                PartnerId = p.PartnerId
            })
            .ToList();

        var policyCount = policyResponses.Count;
        var totalAmount = policyResponses.Sum(p => p.Amount);

        return new PartnerDetailResponse
        {
            Id = partner.Id,
            FullName = $"{partner.FirstName} {partner.LastName}",
            Address = partner.Address,
            PartnerNumber = partner.PartnerNumber,
            CroatianPIN = partner.CroatianPIN,
            PartnerTypeName = partner.PartnerTypeId.ToDisplayName(),
            CreatedAtUtc = partner.CreatedAtUtc,
            CreateByUser = partner.CreateByUser,
            IsForeign = partner.IsForeign,
            ExternalCode = partner.ExternalCode,
            Gender = partner.Gender.ToString(),
            IsFlagged = IsFlagged(policyCount, totalAmount),
            Policies = policyResponses
        };
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

        var partner = new Partner
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Address = request.Address?.Trim(),
            PartnerNumber = request.PartnerNumber,
            CroatianPIN = request.CroatianPIN,
            PartnerTypeId = request.PartnerTypeId!.Value,
            CreateByUser = request.CreateByUser.Trim(),
            IsForeign = request.IsForeign!.Value,
            ExternalCode = request.ExternalCode,
            Gender = request.Gender!.Value
        };

        var newId = await _partnerRepository.InsertPartnerAsync(partner, cancellationToken);

        return Result<int>.Ok(newId);
    }

    private static bool IsFlagged(int policyCount, decimal totalAmount) =>
        PartnerFlagRules.IsFlagged(policyCount, totalAmount);
}
