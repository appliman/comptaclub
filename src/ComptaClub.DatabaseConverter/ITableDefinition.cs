using ComptaClub.EntityFramework;

namespace ComptaClub.DatabaseConverter;

internal interface ITableDefinition
{
    string Name { get; }

    bool HasIdentityKey { get; }
    bool IsOptional { get; }

    Task ValidateAsync(ComptaClubDbContext db, CancellationToken cancellationToken);

    Task<long> CountAsync(ComptaClubDbContext db, CancellationToken cancellationToken);

    Task CopyAsync(
        ComptaClubDbContext source,
        ComptaClubDbContext destination,
        long total,
        TableProgress progress,
        CancellationToken cancellationToken);
}
