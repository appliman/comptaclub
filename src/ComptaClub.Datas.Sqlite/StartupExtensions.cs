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

        var sqliteConnectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        var dataSource = sqliteConnectionStringBuilder.DataSource;
        var folder = Path.GetDirectoryName(dataSource)!;

		var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
		var currentFolder = Path.GetDirectoryName(entryAssembly!.Location)!;
		if (folder.StartsWith("/")
	        || folder.StartsWith(@"\"))
		{
			folder = Path.Combine(currentFolder, folder.Trim('/').Trim('\\'));
		}

		System.IO.Directory.CreateDirectory(folder);
        sqliteConnectionStringBuilder.DataSource = Path.Combine(folder, Path.GetFileName(dataSource));
        connectionString = sqliteConnectionStringBuilder.ToString();

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
        return services;
    }
}
