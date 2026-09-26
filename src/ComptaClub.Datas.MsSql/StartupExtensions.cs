using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Datas.MsSql;

public static class StartupExtensions
{
    public static IServiceCollection AddComptaClubMsSql(this IServiceCollection services, string connectionString, string environmentName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("SQL Server connection string is required.", nameof(connectionString));

        services.AddDbContextFactory<ComptaClubDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.AddInterceptors(new VarcharOptimizationInterceptor());
            if (!environmentName.Contains("prod", StringComparison.OrdinalIgnoreCase))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        }, ServiceLifetime.Singleton);
        services.AddSingleton<IComptaClubDbContextFactory>(provider =>
            new MsSqlDbContextFactory(connectionString, provider.GetRequiredService<IDbContextFactory<ComptaClubDbContext>>()));
        return services;
    }
}
