using ComptaClub.EntityFramework;
using EFScriptableMigration;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ComptaClub.Datas.MsSql;

public sealed class MsSqlDbContextFactory(
    string connectionString,
    IDbContextFactory<ComptaClubDbContext> dbContextFactory) : IComptaClubDbContextFactory
{
    public ComptaClubDbContext CreateDbContext() => dbContextFactory.CreateDbContext();

    public Task<ComptaClubDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        dbContextFactory.CreateDbContextAsync(cancellationToken);

    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var migration = new DbMigration
        {
            ConnectionString = NormalizeLegacyConnectionString(connectionString),
            SchemaName = "ComptaClub",
            EmbededTypeReference = typeof(StartupExtensions)
        };
        return migration.Start();
    }

    private static string NormalizeLegacyConnectionString(string value)
    {
        var parsed = new DbConnectionStringBuilder { ConnectionString = value };
        var legacy = new DbConnectionStringBuilder();
        foreach (string originalKey in parsed.Keys)
        {
            var key = originalKey;
            if (key.Equals("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
            {
                key = "TrustServerCertificate";
            }

            legacy[key] = parsed[originalKey];
        }

        return legacy.ConnectionString;
    }
}
