using System.Data;
using Dapper;
using Partners.Core.Contracts;
using Partners.Core.Models;
using Partners.Dal.Database;

namespace Partners.Dal.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PolicyRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<Policy>> FetchAllPoliciesByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            "dbo.GetPoliciesByPartnerId",
            new { PartnerId = partnerId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<Policy>(command);
    }

    public async Task<int> InsertPolicyAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            policy.PolicyNumber,
            policy.Amount,
            policy.PartnerId
        };

        var command = new CommandDefinition(
            "dbo.CreatePolicy",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<int>(command);
    }

    public async Task<PolicySummary> FetchPolicySummaryByPartnerIdAsync(int partnerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            "dbo.GetPolicySummaryByPartnerId",
            new { PartnerId = partnerId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<PolicySummary>(command);
    }
}
