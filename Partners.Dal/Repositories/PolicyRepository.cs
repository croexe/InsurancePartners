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

        public async Task<IEnumerable<Policy>> GetByPartnerIdAsync(int partnerId)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            SELECT Id, PolicyNumber, Amount, PartnerId
            FROM dbo.Policy
            WHERE PartnerId = @PartnerId";

            return await connection.QueryAsync<Policy>(sql, new { PartnerId = partnerId });
        }

        public async Task<int> CreateAsync(Policy policy)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            INSERT INTO dbo.Policy (PolicyNumber, Amount, PartnerId)
            OUTPUT INSERTED.Id
            VALUES (@PolicyNumber, @Amount, @PartnerId)";

            var parameters = new
            {
                policy.PolicyNumber,
                policy.Amount,
                policy.PartnerId
            };

            return await connection.QuerySingleAsync<int>(sql, parameters);
        }

        public async Task<IReadOnlyDictionary<int, PartnerPolicySummaryResponse>> GetSummariesForAllPartnersAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            SELECT
                PartnerId,
                COUNT(1)            AS PolicyCount,
                ISNULL(SUM(Amount), 0) AS TotalAmount
            FROM dbo.Policy
            GROUP BY PartnerId";

            var rows = await connection.QueryAsync<PartnerPolicySummaryResponse>(sql);

            return rows.ToDictionary(r => r.PartnerId, r => r);
        }
    }
}
