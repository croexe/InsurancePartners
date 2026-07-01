using Microsoft.EntityFrameworkCore;
using Partners.Core.Contracts;
using Partners.Dal.Database;
using Partners.Dal.Repositories;

namespace Partners.Api.Extensions.Configurations;

internal static class PersistenceExtensions
{
    // Pristup podacima: EF Core DbContext (Identity) + Dapper connection factory + repozitoriji.
    private const string ConnectionStringName = "InsurancePartnersDb";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(ConnectionStringName)));

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlConnectionFactory(
                configuration.GetConnectionString(ConnectionStringName)!));

        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();

        return services;
    }
}
