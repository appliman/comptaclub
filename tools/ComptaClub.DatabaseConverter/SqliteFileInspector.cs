using Microsoft.Data.Sqlite;

namespace ComptaClub.DatabaseConverter;

internal static class SqliteFileInspector
{
    public static async Task<string> ValidateSourceAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Le fichier SQLite est introuvable.", fullPath);
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        };
        await using var connection = new SqliteConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check";
        var result = (string?)await command.ExecuteScalarAsync(cancellationToken);
        if (result != "ok")
        {
            throw new InvalidOperationException($"Le contrôle d'intégrité SQLite a échoué : {result ?? "aucune réponse"}.");
        }

        return builder.ConnectionString;
    }

    public static string CreateWritableConnectionString(string path)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ConnectionString;
    }

    public static async Task PrepareStandaloneFileAsync(string path, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(CreateWritableConnectionString(path));
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE)";
        await command.ExecuteNonQueryAsync(cancellationToken);
        command.CommandText = "PRAGMA journal_mode=DELETE";
        await command.ExecuteScalarAsync(cancellationToken);
    }
}
