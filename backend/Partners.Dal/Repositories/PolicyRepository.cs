using Dapper;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Responses;
using Partners.Core.Models;
using Partners.Dal.Database;

namespace Partners.Dal.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PolicyRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Policy>> GetAllPoliciesByPartnerIdAsync(int partnerId)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            return await connection.QueryAsync<Policy>(
                "dbo.GetPoliciesByPartnerId",
                new { PartnerId = partnerId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreatePolicyAsync(Policy policy)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var parameters = new
            {
                policy.PolicyNumber,
                policy.Amount,
                policy.PartnerId
            };

            return await connection.QuerySingleAsync<int>(
                "dbo.CreatePolicy",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<PartnerPolicySummaryResponse> GetPolicySummaryByPartnerIdAsync(int partnerId)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            return await connection.QuerySingleAsync<PartnerPolicySummaryResponse>(
                "dbo.GetPolicySummaryByPartnerId",
                new { PartnerId = partnerId },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
