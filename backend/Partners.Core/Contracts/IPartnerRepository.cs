using Partners.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Core.Contracts
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<Partner>> GetAllPartnersAsync();
        Task<Partner?> GetPartnerByIdAsync(int id);
        Task<int> CreatePartnerAsync(Partner partner);
        Task<bool> ExternalCodeExistsAsync(string externalCode);
    }
}
