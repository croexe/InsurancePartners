using Partners.Core.DTOs.Requests;
using Partners.Core.Results;

namespace Partners.Core.Contracts;

public interface IPolicyService
{
    Task<Result<PolicyCreated>> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default);
}
