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

        public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var rows = await connection.QueryAsync<PartnerRow>(
                "dbo.GetAllPartnersWithPolicySummeries",
                commandType: System.Data.CommandType.StoredProcedure);

            return rows.Select(row => row.ToPartner());
        }

        public async Task<Partner?> GetPartnerByIdAsync(int id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var row = await connection.QuerySingleOrDefaultAsync<PartnerRow>(
                "dbo.GetPartnerById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);

            return row?.ToPartner();
        }

        public async Task<int> CreatePartnerAsync(Partner partner)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

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

            return await connection.QuerySingleAsync<int>(
                "dbo.CreatePartner",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<bool> ExternalCodeExistsAsync(string externalCode)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var count = await connection.QuerySingleAsync<int>(
                "dbo.ExternalCodeExists",
                new { ExternalCode = externalCode },
                commandType: System.Data.CommandType.StoredProcedure);

            return count > 0;
        }
    }
}
