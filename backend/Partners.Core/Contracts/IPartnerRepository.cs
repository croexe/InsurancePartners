using Partners.Core.Models;

namespace Partners.Core.Contracts
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<PartnerWithSummary>> FetchAllPartnersAsync();
        Task<Partner?> FetchPartnerByIdAsync(int id);
        Task<int> InsertPartnerAsync(Partner partner);
        Task<bool> ExternalCodeExistsAsync(string externalCode);
    }
}
