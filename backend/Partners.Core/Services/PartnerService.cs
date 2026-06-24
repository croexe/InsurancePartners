using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Models.Enums;
using Partners.Core.Models.Rules.Partner;
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

    public async Task<IEnumerable<PartnerListItemResponse>> GetAllPartnersAsync()
    {
        var partners = await _partnerRepository.GetAllPartnersAsync();

        return partners.Select(partner => new PartnerListItemResponse
        {
            Id = partner.Id,
            FullName = $"{partner.FirstName} {partner.LastName}",
            PartnerNumber = partner.PartnerNumber,
            CroatianPIN = partner.CroatianPIN,
            PartnerTypeName = SetPartnerType(partner.PartnerTypeId),
            CreatedAtUtc = partner.CreatedAtUtc,
            IsForeign = partner.IsForeign,
            Gender = partner.Gender.ToString(),
            IsFlagged = IsFlagged(partner.PolicyCount, partner.TotalAmount)
        });
    }

    public async Task<PartnerDetailResponse?> GetByIdAsync(int id)
    {
        var partner = await _partnerRepository.GetPartnerByIdAsync(id);
        if (partner is null)
        {
            return null;
        }

        var policies = await _policyRepository.GetByPartnerIdAsync(id);
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
            PartnerTypeName = SetPartnerType(partner.PartnerTypeId),
            CreatedAtUtc = partner.CreatedAtUtc,
            CreateByUser = partner.CreateByUser,
            IsForeign = partner.IsForeign,
            ExternalCode = partner.ExternalCode,
            Gender = partner.Gender.ToString(),
            IsFlagged = IsFlagged(policyCount, totalAmount),
            Policies = policyResponses
        };
    }

    public async Task<PartnerServiceResult> CreateAsync(CreatePartnerRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ExternalCode))
        {
            var exists = await _partnerRepository.ExternalCodeExistsAsync(request.ExternalCode);
            if (exists)
            {
                return PartnerServiceResult.Fail($"ExternalCode '{request.ExternalCode}' is already in use.");
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

        var newId = await _partnerRepository.CreatePartnerAsync(partner);

        return PartnerServiceResult.Ok(newId);
    }

    private static bool IsFlagged(int policyCount, decimal totalAmount) =>
        PartnerFlagRules.IsFlagged(policyCount, totalAmount);

    private static (int PolicyCount, decimal TotalAmount) GetSummaryOrDefault(
        IReadOnlyDictionary<int, PartnerPolicySummaryResponse> summaries, int partnerId)
    {
        return summaries.TryGetValue(partnerId, out var summary)
            ? (summary.PolicyCount, summary.TotalAmount)
            : (0, 0m);
    }

    private static string SetPartnerType(PartnerType partnerType) =>
        partnerType == PartnerType.Personal ? "Privatna osoba" : "Pravna osoba";
}
