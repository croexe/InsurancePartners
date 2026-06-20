using Partners.Core.Models;

namespace Partners.Core.Contracts
{
    public interface IPolicyRepository
    {
        Task<IEnumerable<Policy>> GetByPartnerIdAsync(int partnerId);
        Task<int> CreateAsync(Policy policy);
        Task<int> GetCountByPartnerIdAsync(int partnerId);
        Task<decimal> GetTotalAmountByPartnerIdAsync(int partnerId);
    }
}
