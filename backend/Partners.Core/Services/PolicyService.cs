using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Models.Rules.Partner;
using Partners.Core.Results;

namespace Partners.Core.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IPartnerRepository _partnerRepository;

    public PolicyService(IPolicyRepository policyRepository, IPartnerRepository partnerRepository)
    {
        _policyRepository = policyRepository;
        _partnerRepository = partnerRepository;
    }

    public async Task<PolicyServiceResult> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var partner = await _partnerRepository.FetchPartnerByIdAsync(request.PartnerId!.Value, cancellationToken);
        if (partner is null)
        {
            return PolicyServiceResult.Fail($"Partner with Id '{request.PartnerId}' does not exist.");
        }

        var policy = new Policy
        {
            PolicyNumber = request.PolicyNumber.Trim(),
            Amount = request.Amount!.Value,
            PartnerId = request.PartnerId!.Value
        };

        var newId = await _policyRepository.InsertPolicyAsync(policy, cancellationToken);

        var response = new PolicyResponse
        {
            Id = newId,
            PolicyNumber = policy.PolicyNumber,
            Amount = policy.Amount,
            PartnerId = policy.PartnerId
        };

        var summary = await _policyRepository.FetchPolicySummaryByPartnerIdAsync(policy.PartnerId, cancellationToken);
        var isFlagged = PartnerFlagRules.IsFlagged(summary.PolicyCount, summary.TotalAmount);

        return PolicyServiceResult.Ok(response, isFlagged);
    }
}
