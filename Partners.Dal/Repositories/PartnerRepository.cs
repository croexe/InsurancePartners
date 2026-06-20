using Dapper;
using Partners.Core.Contracts;
using Partners.Core.Models;
using Partners.Dal.Database;

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

            const string sql = @"
            SELECT
                Id, FirstName, LastName, Address, PartnerNumber, CroatianPIN,
                PartnerTypeId, CreatedAtUtc, CreateByUser, IsForeign, ExternalCode, Gender
            FROM dbo.Partner
            ORDER BY CreatedAtUtc DESC";

            return await connection.QueryAsync<Partner>(sql);
        }

        public async Task<Partner?> GetByIdAsync(int id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            const string sql = @"
            SELECT
                Id, FirstName, LastName, Address, PartnerNumber, CroatianPIN,
                PartnerTypeId, CreatedAtUtc, CreateByUser, IsForeign, ExternalCode, Gender
            FROM dbo.Partner
            WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<Partner>(sql, new { Id = id });
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
