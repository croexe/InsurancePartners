using Partners.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Core.Contracts
{
    public interface IPartnerRepository
    {
        Task<IEnumerable<Partner>> GetAllAsync();
        Task<Partner?> GetByIdAsync(int id);
        Task<int> CreateAsync(Partner partner);
        Task<bool> ExternalCodeExistsAsync(string externalCode);
    }
}
