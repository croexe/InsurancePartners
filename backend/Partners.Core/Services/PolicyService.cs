using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Models.Rules.Partner;
using Partners.Core.Results;

namespace Partners.Core.Services
{
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

        public async Task<PolicyServiceResult> CreateAsync(CreatePolicyRequest request)
        {
            var errors = new List<string>();

            if (request.Amount is null)
            {
                errors.Add("Amount is required.");
            }

            if (request.PartnerId is null)
            {
                errors.Add("PartnerId is required.");
            }

            if (errors.Count > 0)
            {
                return PolicyServiceResult.Fail(errors.ToArray());
            }

            var partner = await _partnerRepository.GetByIdAsync(request.PartnerId!.Value);
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

            var newId = await _policyRepository.CreateAsync(policy);

            var response = new PolicyResponse
            {
                Id = newId,
                PolicyNumber = policy.PolicyNumber,
                Amount = policy.Amount,
                PartnerId = policy.PartnerId
            };

            var allPartnerPolicies = await _policyRepository.GetByPartnerIdAsync(policy.PartnerId);
            var newPolicyCount = allPartnerPolicies.Count();
            var newTotalAmount = allPartnerPolicies.Sum(p => p.Amount);
            var isFlagged = PartnerFlagRules.IsFlagged(newPolicyCount, newTotalAmount);

            await _partnerNotifier.NotifyPartnerFlagChangedAsync(policy.PartnerId, isFlagged);

            return PolicyServiceResult.Ok(response);
        }
    }
}
