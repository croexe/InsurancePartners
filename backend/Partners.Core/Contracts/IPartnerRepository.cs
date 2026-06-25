using Partners.Core.Models;

namespace Partners.Core.Contracts
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<PartnerWithSummary>> FetchAllPartnersAsync(CancellationToken cancellationToken = default);
        Task<Partner?> FetchPartnerByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> InsertPartnerAsync(Partner partner, CancellationToken cancellationToken = default);
        Task<bool> ExternalCodeExistsAsync(string externalCode, CancellationToken cancellationToken = default);
    }
}
