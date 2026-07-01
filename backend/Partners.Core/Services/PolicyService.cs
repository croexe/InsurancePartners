using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Mapping;
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

    public async Task<Result<PolicyCreated>> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var partner = await _partnerRepository.FetchPartnerByIdAsync(request.PartnerId!.Value, cancellationToken);
        if (partner is null)
        {
            return Result<PolicyCreated>.Fail($"Partner with Id '{request.PartnerId}' does not exist.");
        }

        var policy = request.ToPolicy();
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

        return Result<PolicyCreated>.Ok(new PolicyCreated(response, isFlagged));
    }
}
