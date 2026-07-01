using Partners.Core.Models;

namespace Partners.Core.Contracts;

public interface IPartnerRepository
{
    Task<(IEnumerable<PartnerWithSummary> Items, int TotalCount)> FetchPartnersPageAsync(int offset, int pageSize, CancellationToken cancellationToken = default);
    Task<Partner?> FetchPartnerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> InsertPartnerAsync(Partner partner, CancellationToken cancellationToken = default);
    Task<bool> PartnerExistsAsync(int id, CancellationToken cancellationToken = default);
}
