using Partners.Core.Models;

namespace Partners.Core.Contracts;

public interface IPolicyRepository
{
    Task<IEnumerable<Policy>> FetchAllPoliciesByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default);
    Task<int> InsertPolicyAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<PolicySummary> FetchPolicySummaryByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default);
}
