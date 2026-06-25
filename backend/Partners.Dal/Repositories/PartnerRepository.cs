using System.Data;
using Dapper;
using Partners.Core.Contracts;
using Partners.Core.Models;
using Partners.Dal.Database;
using Partners.Dal.Helpers;

namespace Partners.Dal.Repositories
{
    public class PartnerRepository : IPartnerRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PartnerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<PartnerWithSummary>> FetchAllPartnersAsync(CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                "dbo.GetAllPartnersWithPolicySummeriesFirstServe",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await connection.QueryAsync<PartnerRow>(command);

            return rows.Select(row => row.ToPartnerWithSummary());
        }

        public async Task<Partner?> FetchPartnerByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

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
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

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

            return await connection.QuerySingleAsync<int>(command);
        }

        public async Task<bool> ExternalCodeExistsAsync(string externalCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var command = new CommandDefinition(
                "dbo.ExternalCodeExists",
                new { ExternalCode = externalCode },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var count = await connection.QuerySingleAsync<int>(command);

            return count > 0;
        }
    }
}
