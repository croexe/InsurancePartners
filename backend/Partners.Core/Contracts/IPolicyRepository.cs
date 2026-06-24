using Partners.Core.DTOs.Responses;
using Partners.Core.Models;

namespace Partners.Core.Contracts
{
    public interface IPolicyRepository
    {
        Task<IEnumerable<Policy>> GetAllPoliciesByPartnerIdAsync(int partnerId);
        Task<int> CreatePolicyAsync(Policy policy);
        Task<PartnerPolicySummaryResponse> GetPolicySummaryByPartnerIdAsync(int partnerId);
    }
}
