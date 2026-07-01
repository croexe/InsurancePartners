using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Models.Rules.Partner;
using Partners.Core.Presentation;

namespace Partners.Core.Mapping;

public static class PartnerMappings
{
    public static Partner ToPartner(this CreatePartnerRequest request) => new()
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

    public static PartnerListItemResponse ToListItemResponse(this PartnerWithSummary item)
    {
        var partner = item.Partner;
        return new PartnerListItemResponse
        {
            Id = partner.Id,
            FullName = $"{partner.FirstName} {partner.LastName}",
            PartnerNumber = partner.PartnerNumber,
            CroatianPIN = partner.CroatianPIN,
            PartnerTypeName = partner.PartnerTypeId.ToDisplayName(),
            CreatedAtUtc = partner.CreatedAtUtc,
            IsForeign = partner.IsForeign,
            Gender = partner.Gender.ToString(),
            IsFlagged = PartnerFlagRules.IsFlagged(item.PolicyCount, item.TotalAmount)
        };
    }

    public static PartnerDetailResponse ToDetailResponse(this Partner partner, IEnumerable<Policy> policies)
    {
        var policyResponses = policies.Select(policy => policy.ToResponse()).ToList();

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
            IsFlagged = PartnerFlagRules.IsFlagged(policyResponses.Count, policyResponses.Sum(policy => policy.Amount)),
            Policies = policyResponses
        };
    }
}
