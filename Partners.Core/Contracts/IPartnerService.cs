using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Results;

namespace Partners.Core.Contracts
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerListItemResponse>> GetAllAsync();
        Task<PartnerDetailResponse?> GetByIdAsync(int id);
        Task<PartnerServiceResult> CreateAsync(CreatePartnerRequest request);
    }
}
