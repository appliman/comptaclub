using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Datas.Sqlite;

public static class StartupExtensions
{
    public static IServiceCollection AddComptaClubSqlite(this IServiceCollection services, string connectionString, string environmentName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("SQLite connection string is required.", nameof(connectionString));
        }

		services.AddDbContextFactory<ComptaClubDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(StartupExtensions).Assembly.FullName));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (!environmentName.Contains("prod", StringComparison.OrdinalIgnoreCase))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        }, ServiceLifetime.Singleton);
        services.AddSingleton<IComptaClubDbContextFactory, SqliteDbContextFactory>();
        services.AddSingleton<IDatabaseBackupService, SqliteDatabaseBackupService>();
        return services;
    }
}
