using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.DatabaseConverter;

internal sealed class TableDefinition<TEntity>(string name, bool hasIdentityKey = false) : ITableDefinition
    where TEntity : class
{
    private const int BATCH_SIZE = 200;

    public string Name { get; } = name;

    public bool HasIdentityKey { get; } = hasIdentityKey;

    public async Task ValidateAsync(ComptaClubDbContext db, CancellationToken cancellationToken)
    {
        await db.Set<TEntity>().AsNoTracking().Take(1).ToListAsync(cancellationToken);
    }

    public Task<long> CountAsync(ComptaClubDbContext db, CancellationToken cancellationToken) =>
        db.Set<TEntity>().LongCountAsync(cancellationToken);

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
}
