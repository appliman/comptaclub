namespace ComptaClub.EntityFramework;

public interface IComptaClubDbContextFactory
{
    ComptaClubDbContext CreateDbContext();
    Task<ComptaClubDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
