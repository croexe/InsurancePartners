using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Results;

namespace Partners.Core.Contracts
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerListItemResponse>> GetAllPartnersAsync();
        Task<PartnerDetailResponse?> GetPartnerDetailsByIdAsync(int id);
        Task<PartnerServiceResult> CreatePartnerAsync(CreatePartnerRequest request);
    }
}
