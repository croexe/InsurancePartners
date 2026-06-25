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
    private readonly IPartnerNotifier _partnerNotifier;

    public PolicyService(IPolicyRepository policyRepository, IPartnerRepository partnerRepository, IPartnerNotifier partnerNotifier)
    {
        _policyRepository = policyRepository;
        _partnerRepository = partnerRepository;
        _partnerNotifier = partnerNotifier;
    }

    public async Task<PolicyServiceResult> CreatePolicyAsync(CreatePolicyRequest request)
    {
        var partner = await _partnerRepository.FetchPartnerByIdAsync(request.PartnerId!.Value);
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

        var newId = await _policyRepository.InsertPolicyAsync(policy);

        var response = new PolicyResponse
        {
            Id = newId,
            PolicyNumber = policy.PolicyNumber,
            Amount = policy.Amount,
            PartnerId = policy.PartnerId
        };

        var summary = await _policyRepository.FetchPolicySummaryByPartnerIdAsync(policy.PartnerId);
        var isFlagged = PartnerFlagRules.IsFlagged(summary.PolicyCount, summary.TotalAmount);

        await _partnerNotifier.NotifyPartnerFlagChangedAsync(policy.PartnerId, isFlagged);

        return PolicyServiceResult.Ok(response);
    }
}
