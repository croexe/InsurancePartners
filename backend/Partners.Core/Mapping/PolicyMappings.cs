using Partners.Core.DTOs.Responses;
using Partners.Core.Models;

namespace Partners.Core.Mapping;

public static class PolicyMappings
{
    public static PolicyResponse ToResponse(this Policy policy) => new()
    {
        Id = policy.Id,
        PolicyNumber = policy.PolicyNumber,
        Amount = policy.Amount,
        PartnerId = policy.PartnerId
    };
}
