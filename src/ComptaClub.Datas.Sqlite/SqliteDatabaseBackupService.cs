using ComptaClub.EntityFramework;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Datas.Sqlite;

public sealed class SqliteDatabaseBackupService(IComptaClubDbContextFactory dbContextFactory) : IDatabaseBackupService
{
    public async Task CreateBackup(string destination, CancellationToken cancellationToken = default)
    {
        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await _db.Database.OpenConnectionAsync(cancellationToken);
        var _source = (SqliteConnection)_db.Database.GetDbConnection();
        await using var _target = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = destination,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ConnectionString);
        await _target.OpenAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _source.BackupDatabase(_target);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
