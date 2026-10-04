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

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var migration = new DbMigration
        {
            ConnectionString = NormalizeLegacyConnectionString(connectionString),
            SchemaName = "ComptaClub",
            EmbededTypeReference = typeof(StartupExtensions)
        };
        await migration.Start();
        var _options = new DbContextOptionsBuilder<McpSchemaDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__McpMigrationsHistory")).Options;
        await using var _db = new McpSchemaDbContext(_options);
        await _db.Database.MigrateAsync(cancellationToken);
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
