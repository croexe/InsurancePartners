using Partners.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Core.Contracts
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<Partner>> FetchAllPartnersAsync();
        Task<Partner?> FetchPartnerByIdAsync(int id);
        Task<int> InsertPartnerAsync(Partner partner);
        Task<bool> ExternalCodeExistsAsync(string externalCode);
    }
}
