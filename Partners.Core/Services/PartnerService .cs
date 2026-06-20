using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Core.Results;
using Partners.Core.Validators;
 
namespace Partners.Core.Services
{
    public class PartnerService : IPartnerService
    {
        private const int MaxPolicyCountBeforeFlag = 5;
        private const decimal MaxPolicyAmountBeforeFlag = 5000m;

        private readonly IPartnerRepository _partnerRepository;
        private readonly IPolicyRepository _policyRepository;

        public PartnerService(IPartnerRepository partnerRepository, IPolicyRepository policyRepository)
        {
            _partnerRepository = partnerRepository;
            _policyRepository = policyRepository;
        }

        public async Task<IEnumerable<PartnerListItemResponse>> GetAllAsync()
        {
            var partners = await _partnerRepository.GetAllAsync();
            var summaries = await _policyRepository.GetSummariesForAllPartnersAsync();

            var result = new List<PartnerListItemResponse>();

            foreach (var partner in partners)
            {
                var (policyCount, totalAmount) = GetSummaryOrDefault(summaries, partner.Id);

                result.Add(new PartnerListItemResponse
                {
                    Id = partner.Id,
                    FullName = $"{partner.FirstName} {partner.LastName}",
                    PartnerNumber = partner.PartnerNumber,
                    CroatianPIN = partner.CroatianPIN,
                    PartnerTypeName = partner.PartnerTypeId.ToString(),
                    CreatedAtUtc = partner.CreatedAtUtc,
                    IsForeign = partner.IsForeign,
                    Gender = partner.Gender.ToString(),
                    IsFlagged = IsFlagged(policyCount, totalAmount)
                });
            }

            return result;
        }

        public async Task<PartnerDetailResponse?> GetByIdAsync(int id)
        {
            var partner = await _partnerRepository.GetByIdAsync(id);
            if (partner is null)
            {
                return null;
            }

            var policies = await _policyRepository.GetByPartnerIdAsync(id);
            var policyResponses = policies
                .Select(p => new PolicyResponse
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    Amount = p.Amount,
                    PartnerId = p.PartnerId
                })
                .ToList();

            var policyCount = policyResponses.Count;
            var totalAmount = policyResponses.Sum(p => p.Amount);

            return new PartnerDetailResponse
            {
                Id = partner.Id,
                FullName = $"{partner.FirstName} {partner.LastName}",
                Address = partner.Address,
                PartnerNumber = partner.PartnerNumber,
                CroatianPIN = partner.CroatianPIN,
                PartnerTypeName = partner.PartnerTypeId.ToString(),
                CreatedAtUtc = partner.CreatedAtUtc,
                CreateByUser = partner.CreateByUser,
                IsForeign = partner.IsForeign,
                ExternalCode = partner.ExternalCode,
                Gender = partner.Gender.ToString(),
                IsFlagged = IsFlagged(policyCount, totalAmount),
                Policies = policyResponses
            };
        }

        public async Task<PartnerServiceResult> CreateAsync(CreatePartnerRequest request)
        {
            var errors = new List<string>();

            if (request.PartnerTypeId is null)
            {
                errors.Add("PartnerTypeId is required.");
            }

            if (request.IsForeign is null)
            {
                errors.Add("IsForeign is required.");
            }

            if (request.Gender is null)
            {
                errors.Add("Gender is required.");
            }

            if (!string.IsNullOrWhiteSpace(request.CroatianPIN) && !CroatianPinValidator.IsValid(request.CroatianPIN))
            {
                errors.Add("CroatianPIN (OIB) is not valid.");
            }

            if (!string.IsNullOrWhiteSpace(request.ExternalCode))
            {
                var exists = await _partnerRepository.ExternalCodeExistsAsync(request.ExternalCode);
                if (exists)
                {
                    errors.Add($"ExternalCode '{request.ExternalCode}' is already in use.");
                }
            }

            if (errors.Count > 0)
            {
                return PartnerServiceResult.Fail(errors.ToArray());
            }

            var partner = new Partner
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

            var newId = await _partnerRepository.CreateAsync(partner);

            return PartnerServiceResult.Ok(newId);
        }

        private static bool IsFlagged(int policyCount, decimal totalAmount) =>
            policyCount > MaxPolicyCountBeforeFlag || totalAmount > MaxPolicyAmountBeforeFlag;

        private static (int PolicyCount, decimal TotalAmount) GetSummaryOrDefault(
            IReadOnlyDictionary<int, PartnerPolicySummaryResponse> summaries, int partnerId)
        {
            return summaries.TryGetValue(partnerId, out var summary)
                ? (summary.PolicyCount, summary.TotalAmount)
                : (0, 0m);
        }
    }
}
