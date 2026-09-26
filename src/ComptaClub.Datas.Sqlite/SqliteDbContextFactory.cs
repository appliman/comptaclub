using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Datas.Sqlite;

public sealed class SqliteDbContextFactory(IDbContextFactory<ComptaClubDbContext> dbContextFactory) : IComptaClubDbContextFactory
{
    public ComptaClubDbContext CreateDbContext() => dbContextFactory.CreateDbContext();

    public Task<ComptaClubDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        dbContextFactory.CreateDbContextAsync(cancellationToken);

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
        await db.Database.OpenConnectionAsync(cancellationToken);
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        await command.ExecuteScalarAsync(cancellationToken);
    }
}
