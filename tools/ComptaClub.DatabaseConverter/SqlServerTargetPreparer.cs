using Microsoft.Data.SqlClient;

namespace ComptaClub.DatabaseConverter;

internal sealed class SqlServerTargetPreparer(string connectionString)
{
    private readonly SqlConnectionStringBuilder _target = new(connectionString);

    public string DatabaseName => _target.InitialCatalog;

    public bool CreatedDatabase { get; private set; }

    public bool HasApplicationSchema { get; private set; }

    public async Task PrepareAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(DatabaseName) ||
            new[] { "master", "model", "msdb", "tempdb" }.Contains(DatabaseName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La chaîne SQL Server doit désigner une nouvelle base utilisateur valide.");
        }

        var administrative = new SqlConnectionStringBuilder(_target.ConnectionString)
        {
            InitialCatalog = "master"
        };
        await using var master = new SqlConnection(administrative.ConnectionString);
        await master.OpenAsync(cancellationToken);

        await using (var exists = master.CreateCommand())
        {
            exists.CommandText = "SELECT CASE WHEN DB_ID(@name) IS NULL THEN 0 ELSE 1 END";
            exists.Parameters.AddWithValue("@name", DatabaseName);
            if ((int)(await exists.ExecuteScalarAsync(cancellationToken) ?? 0) == 0)
            {
                await using var create = master.CreateCommand();
                create.CommandText = $"CREATE DATABASE [{DatabaseName.Replace("]", "]]", StringComparison.Ordinal)}]";
                await create.ExecuteNonQueryAsync(cancellationToken);
                CreatedDatabase = true;
                return;
            }
        }

        await using var target = new SqlConnection(_target.ConnectionString);
        await target.OpenAsync(cancellationToken);
        var applicationTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userTableCount = 0;
        await using (var command = target.CreateCommand())
        {
            command.CommandText = "SELECT SCHEMA_NAME(schema_id), name FROM sys.tables WHERE is_ms_shipped = 0";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                userTableCount++;
                if (reader.GetString(0).Equals("dbo", StringComparison.OrdinalIgnoreCase))
                {
                    applicationTables.Add(reader.GetString(1));
                }
            }
        }

        var applicationCount = TableCatalog.Tables.Count(table => applicationTables.Contains(table.Name));
        if (applicationCount > 0 && applicationCount < TableCatalog.Tables.Count)
        {
            throw new InvalidOperationException("La base SQL Server existante possède un schéma ComptaClub incomplet.");
        }

        if (applicationCount == 0 && userTableCount > 0)
        {
            throw new InvalidOperationException("La base SQL Server existante contient des tables sans schéma ComptaClub.");
        }

        HasApplicationSchema = applicationCount == TableCatalog.Tables.Count;
    }

    public async Task DropCreatedDatabaseAsync()
    {
        if (!CreatedDatabase)
        {
            return;
        }

        var administrative = new SqlConnectionStringBuilder(_target.ConnectionString)
        {
            InitialCatalog = "master"
        };
        await using var master = new SqlConnection(administrative.ConnectionString);
        await master.OpenAsync();
        await using var command = master.CreateCommand();
        var escapedName = DatabaseName.Replace("]", "]]", StringComparison.Ordinal);
        command.CommandText = $"ALTER DATABASE [{escapedName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{escapedName}];";
        await command.ExecuteNonQueryAsync();
        CreatedDatabase = false;
    }
}
