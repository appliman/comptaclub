using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComptaClub.DatabaseConverter;

internal sealed class TableDefinition<TEntity>(string name, bool hasIdentityKey = false, bool isOptional = false) : ITableDefinition
    where TEntity : class
{
    private const int BATCH_SIZE = 200;

    public string Name { get; } = name;

    public bool HasIdentityKey { get; } = hasIdentityKey;
    public bool IsOptional { get; } = isOptional;

    public async Task ValidateAsync(ComptaClubDbContext db, CancellationToken cancellationToken)
    {
        if (IsOptional && !await Exists(db, cancellationToken))
        {
            return;
        }
        await db.Set<TEntity>().AsNoTracking().Take(1).ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(ComptaClubDbContext db, CancellationToken cancellationToken)
    {
        if (IsOptional && !await Exists(db, cancellationToken))
        {
            return 0;
        }
        return await db.Set<TEntity>().LongCountAsync(cancellationToken);
    }

    public async Task CopyAsync(
        ComptaClubDbContext source,
        ComptaClubDbContext destination,
        long total,
        TableProgress progress,
        CancellationToken cancellationToken)
    {
        var copied = 0L;
        var pending = 0;
        var batchSize = Name == "DocumentContents" ? 1 : BATCH_SIZE;

        progress.Report(Name, copied, total);
        if (IsOptional && total == 0)
        {
            progress.Report(Name, 0, 0, completed: true);
            return;
        }
        await foreach (var entity in source.Set<TEntity>().AsNoTracking().AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            destination.Set<TEntity>().Add(entity);
            pending++;
            if (pending < batchSize)
            {
                continue;
            }

            await destination.SaveChangesAsync(cancellationToken);
            destination.ChangeTracker.Clear();
            copied += pending;
            pending = 0;
            progress.Report(Name, copied, total);
        }

        if (pending > 0)
        {
            await destination.SaveChangesAsync(cancellationToken);
            destination.ChangeTracker.Clear();
            copied += pending;
        }

        progress.Report(Name, copied, total, completed: true);
        if (copied != total)
        {
            throw new InvalidOperationException($"La table {Name} a changé pendant la copie ({total} lignes attendues, {copied} copiées).");
        }
    }

    private async Task<bool> Exists(ComptaClubDbContext db, CancellationToken cancellationToken)
    {
        var _connection = db.Database.GetDbConnection();
        var _opened = _connection.State != System.Data.ConnectionState.Open;
        if (_opened)
        {
            await db.Database.OpenConnectionAsync(cancellationToken);
        }
        try
        {
            await using var _command = _connection.CreateCommand();
            _command.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
            _command.CommandText = db.Database.IsSqlite()
                ? "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name"
                : "SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND name = @name";
            var _parameter = _command.CreateParameter();
            _parameter.ParameterName = "@name";
            _parameter.Value = Name;
            _command.Parameters.Add(_parameter);
            return Convert.ToInt32(await _command.ExecuteScalarAsync(cancellationToken)) == 1;
        }
        finally
        {
            if (_opened)
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
