using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Partners.Core.Contracts;
using Partners.Core.Exceptions;
using Partners.Core.Models;
using Partners.Dal.Database;
using Partners.Dal.Helpers;

namespace Partners.Dal.Repositories;

public class PartnerRepository : IPartnerRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public PartnerRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<PartnerWithSummary>> FetchAllPartnersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            "dbo.GetAllPartnersWithPolicySummeriesFirstServe",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PartnerRow>(command);

        return rows.Select(row => row.ToPartnerWithSummary());
    }

    public async Task<Partner?> FetchPartnerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            "dbo.GetPartnerById",
            new { Id = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<PartnerRow>(command);

        return row?.ToPartner();
    }

    public async Task<int> InsertPartnerAsync(Partner partner, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new
        {
            partner.FirstName,
            partner.LastName,
            partner.Address,
            partner.PartnerNumber,
            partner.CroatianPIN,
            PartnerTypeId = (int)partner.PartnerTypeId,
            partner.CreateByUser,
            partner.IsForeign,
            partner.ExternalCode,
            Gender = partner.Gender.ToString()
        };

        var command = new CommandDefinition(
            "dbo.CreatePartner",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        try
        {
            return await connection.QuerySingleAsync<int>(command);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new DuplicateExternalCodeException(partner.ExternalCode!);
        }
    }

    public async Task<bool> PartnerExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            "dbo.PartnerExists",
            new { Id = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<bool>(command);
    }
}
