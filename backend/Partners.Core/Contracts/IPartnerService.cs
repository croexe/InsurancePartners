using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;
using Partners.Core.Results;

namespace Partners.Core.Contracts;

public interface IPartnerService
{
    Task<PagedResponse<PartnerListItemResponse>> GetPartnersPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PartnerDetailResponse?> GetPartnerDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreatePartnerAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default);
}
