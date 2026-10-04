namespace ComptaClub.EntityFramework;

public interface IDatabaseBackupService
{
    Task CreateBackup(string destination, CancellationToken cancellationToken = default);
}
