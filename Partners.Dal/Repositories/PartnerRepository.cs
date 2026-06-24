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

        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var rows = await connection.QueryAsync<PartnerRow>(
                "dbo.GetAllPartners",
                commandType: System.Data.CommandType.StoredProcedure);

            return rows.Select(row => row.ToPartner());
        }

        public async Task<Partner?> GetByIdAsync(int id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var row = await connection.QuerySingleOrDefaultAsync<PartnerRow>(
                "dbo.GetPartnerById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);

            return row?.ToPartner();
        }

        public async Task<int> CreateAsync(Partner partner)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            INSERT INTO dbo.Partner
                (FirstName, LastName, Address, PartnerNumber, CroatianPIN,
                 PartnerTypeId, CreateByUser, IsForeign, ExternalCode, Gender)
            OUTPUT INSERTED.Id
            VALUES
                (@FirstName, @LastName, @Address, @PartnerNumber, @CroatianPIN,
                 @PartnerTypeId, @CreateByUser, @IsForeign, @ExternalCode, @Gender)";

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

            return await connection.QuerySingleAsync<int>(sql, parameters);
        }

        public async Task<bool> ExternalCodeExistsAsync(string externalCode)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            SELECT COUNT(1)
            FROM dbo.Partner
            WHERE ExternalCode = @ExternalCode";

            var count = await connection.QuerySingleAsync<int>(sql, new { ExternalCode = externalCode });
            return count > 0;
        }
    }
}
