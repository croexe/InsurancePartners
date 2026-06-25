using Partners.Core.DTOs.Responses;
using Partners.Core.Models;

namespace Partners.Core.Contracts
{
    public interface IPolicyRepository
    {
        Task<IEnumerable<Policy>> FetchAllPoliciesByPartnerIdAsync(int partnerId);
        Task<int> InsertPolicyAsync(Policy policy);
        Task<PartnerPolicySummaryResponse> FetchPolicySummaryByPartnerIdAsync(int partnerId);
        Task<IReadOnlyDictionary<int, PartnerPolicySummaryResponse>> GetSummariesForPartnerAsync();
    }
}
