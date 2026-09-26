using ComptaClub.EntityFramework;
using EFScriptableMigration;
using Microsoft.EntityFrameworkCore;

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
            ConnectionString = connectionString,
            SchemaName = "ComptaClub",
            EmbededTypeReference = typeof(StartupExtensions)
        };
        return migration.Start();
    }
}
