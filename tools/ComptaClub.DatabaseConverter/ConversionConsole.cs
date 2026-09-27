using ComptaClub.Datas.MsSql;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.DatabaseConverter;

internal sealed class ConversionConsole
{
    private string? _sqlConnectionString;

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Conversion des bases ComptaClub");
        Console.WriteLine("Arrêtez l'application qui écrit dans la base source avant de continuer.");
        Console.WriteLine("1 - MSSql vers Sqlite");
        Console.WriteLine("2 - Sqlite vers MSSql");

        try
        {
            var choice = ReadChoice();
            _sqlConnectionString = ReadRequired("Collez la chaîne de connexion SQL Server : ");
            _ = new SqlConnectionStringBuilder(_sqlConnectionString);

            if (choice == "1")
            {
                await ConvertToSqliteAsync(cancellationToken);
            }
            else
            {
                await ConvertToSqlServerAsync(cancellationToken);
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Conversion annulée.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Conversion arrêtée : {SafeError(ex)}");
            return 1;
        }
    }

    private async Task ConvertToSqliteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Vérification de la base SQL Server source...");
        await using (var connection = new SqlConnection(_sqlConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var sourceServices = CreateSqlServerServices(_sqlConnectionString!);
        var sourceFactory = sourceServices.GetRequiredService<IComptaClubDbContextFactory>();
        var copyService = new DataCopyService();
        IReadOnlyList<TableCopyResult> sourceTables;
        await using (var source = await sourceFactory.CreateDbContextAsync(cancellationToken))
        {
            sourceTables = await copyService.InspectSourceAsync(source, cancellationToken);
        }

        var destinationPath = ReadSqliteDestination();
        var temporaryPath = Path.Combine(
            Path.GetDirectoryName(destinationPath)!,
            $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            var sqliteConnectionString = SqliteFileInspector.CreateWritableConnectionString(temporaryPath);
            using (var destinationServices = CreateSqliteServices(sqliteConnectionString))
            {
                var destinationFactory = destinationServices.GetRequiredService<IComptaClubDbContextFactory>();
                await SchemaProgress.RunAsync("SQLite", () => destinationFactory.MigrateAsync(cancellationToken), cancellationToken);

                await using var source = await sourceFactory.CreateDbContextAsync(cancellationToken);
                await using var destination = await destinationFactory.CreateDbContextAsync(cancellationToken);
                await copyService.CopyAsync(source, destination, sourceTables, false, cancellationToken);
            }

            await SqliteFileInspector.PrepareStandaloneFileAsync(temporaryPath, cancellationToken);
            await SqliteFileInspector.ValidateSourceAsync(temporaryPath, cancellationToken);
            if (File.Exists(destinationPath))
            {
                File.Replace(temporaryPath, destinationPath, null);
            }
            else
            {
                File.Move(temporaryPath, destinationPath);
            }

            PrintSummary("SQL Server → SQLite", destinationPath, sourceTables);
        }
        finally
        {
            DeleteTemporaryFiles(temporaryPath);
        }
    }

    private async Task ConvertToSqlServerAsync(CancellationToken cancellationToken)
    {
        var sqlitePath = ReadRequired("Chemin de la base SQLite source : ").Trim('"');
        Console.WriteLine("Vérification du fichier SQLite source...");
        var sqliteConnectionString = await SqliteFileInspector.ValidateSourceAsync(sqlitePath, cancellationToken);
        using var sourceServices = CreateSqliteServices(sqliteConnectionString);
        var sourceFactory = sourceServices.GetRequiredService<IComptaClubDbContextFactory>();
        var copyService = new DataCopyService();
        IReadOnlyList<TableCopyResult> sourceTables;
        await using (var source = await sourceFactory.CreateDbContextAsync(cancellationToken))
        {
            sourceTables = await copyService.InspectSourceAsync(source, cancellationToken);
        }

        var target = new SqlServerTargetPreparer(_sqlConnectionString!);
        await target.PrepareAsync(cancellationToken);
        try
        {
            using var destinationServices = CreateSqlServerServices(_sqlConnectionString!);
            var destinationFactory = destinationServices.GetRequiredService<IComptaClubDbContextFactory>();
            if (target.HasApplicationSchema)
            {
                await using var existing = await destinationFactory.CreateDbContextAsync(cancellationToken);
                await copyService.EnsureDestinationEmptyAsync(existing, cancellationToken);
            }

            await SchemaProgress.RunAsync("SQL Server", () => destinationFactory.MigrateAsync(cancellationToken), cancellationToken);

            await using var source = await sourceFactory.CreateDbContextAsync(cancellationToken);
            await using var destination = await destinationFactory.CreateDbContextAsync(cancellationToken);
            await copyService.EnsureDestinationEmptyAsync(destination, cancellationToken);
            await copyService.CopyAsync(source, destination, sourceTables, true, cancellationToken);
            PrintSummary("SQLite → SQL Server", target.DatabaseName, sourceTables);
        }
        catch
        {
            if (target.CreatedDatabase)
            {
                try
                {
                    await target.DropCreatedDatabaseAsync();
                    Console.Error.WriteLine("La base SQL Server créée pour cette tentative a été supprimée.");
                }
                catch (Exception cleanupError)
                {
                    Console.Error.WriteLine($"Suppression de la base SQL Server incomplète : {SafeError(cleanupError)}");
                }
            }

            throw;
        }
    }

    private static ServiceProvider CreateSqlServerServices(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddComptaClubMsSql(connectionString, "Production");
        return services.BuildServiceProvider();
    }

    private static ServiceProvider CreateSqliteServices(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddComptaClubSqlite(connectionString, "Production");
        return services.BuildServiceProvider();
    }

    private static string ReadChoice()
    {
        while (true)
        {
            var choice = ReadRequired("Votre choix (1 ou 2) : ");
            if (choice is "1" or "2")
            {
                return choice;
            }

            Console.WriteLine("Choisissez 1 ou 2.");
        }
    }

    private static string ReadSqliteDestination()
    {
        var defaultDirectory = AppContext.BaseDirectory;
        Console.Write($"Dossier de destination [{defaultDirectory}] : ");
        var directoryInput = Console.ReadLine() ?? throw new OperationCanceledException();
        var directory = Path.GetFullPath(string.IsNullOrWhiteSpace(directoryInput) ? defaultDirectory : directoryInput.Trim().Trim('"'));
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Le dossier de destination est introuvable : {directory}");
        }

        Console.Write("Nom du fichier [comptaclub.db] : ");
        var fileInput = Console.ReadLine() ?? throw new OperationCanceledException();
        var name = string.IsNullOrWhiteSpace(fileInput) ? "comptaclub.db" : fileInput.Trim();
        if (name != Path.GetFileName(name) || !name.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Le nom doit être un fichier .db sans chemin de dossier.");
        }

        var path = Path.Combine(directory, name);
        Console.WriteLine($"Destination SQLite : {path}");
        if (!Confirm("Valider cette destination ?"))
        {
            throw new OperationCanceledException();
        }

        if (File.Exists(path))
        {
            if (!Confirm("Le fichier existe déjà. Confirmer son écrasement ?"))
            {
                throw new OperationCanceledException();
            }

            if (File.Exists(path + "-wal") || File.Exists(path + "-shm"))
            {
                throw new InvalidOperationException("Des fichiers SQLite WAL/SHM sont présents à destination. Arrêtez l'application et consolidez la base avant l'écrasement.");
            }
        }

        return path;
    }

    private static bool Confirm(string question)
    {
        Console.Write($"{question} (o/N) : ");
        var response = Console.ReadLine()?.Trim();
        return response is not null &&
               (response.Equals("o", StringComparison.OrdinalIgnoreCase) ||
                response.Equals("oui", StringComparison.OrdinalIgnoreCase));
    }

    private static string ReadRequired(string question)
    {
        Console.Write(question);
        var value = Console.ReadLine();
        if (value is null)
        {
            throw new OperationCanceledException();
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Une valeur est requise.");
        }

        return value.Trim();
    }

    private static void PrintSummary(string direction, string destination, IReadOnlyList<TableCopyResult> tables)
    {
        Console.WriteLine();
        Console.WriteLine("Conversion terminée avec succès.");
        Console.WriteLine($"Sens : {direction}");
        Console.WriteLine($"Destination : {destination}");
        Console.WriteLine($"Tables copiées : {tables.Count}");
        Console.WriteLine($"Lignes copiées : {tables.Sum(table => table.RowCount)}");
        foreach (var table in tables)
        {
            Console.WriteLine($"  {table.Name} : {table.RowCount}");
        }
    }

    private string SafeError(Exception error)
    {
        var messages = new List<string>();
        for (Exception? current = error; current is not null; current = current.InnerException)
        {
            if (!messages.Contains(current.Message, StringComparer.Ordinal))
            {
                messages.Add(current.Message);
            }
        }

        var message = string.Join(" → ", messages);
        if (string.IsNullOrEmpty(_sqlConnectionString))
        {
            return message;
        }

        message = message.Replace(_sqlConnectionString, "[chaîne SQL Server masquée]", StringComparison.OrdinalIgnoreCase);
        try
        {
            var builder = new SqlConnectionStringBuilder(_sqlConnectionString);
            if (!string.IsNullOrEmpty(builder.Password))
            {
                message = message.Replace(builder.Password, "[mot de passe masqué]", StringComparison.Ordinal);
            }
        }
        catch (ArgumentException)
        {
            // The invalid connection string is already replaced above.
        }

        return message;
    }

    private static void DeleteTemporaryFiles(string path)
    {
        foreach (var file in new[] { path, path + "-wal", path + "-shm" })
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    Console.Error.WriteLine($"Le fichier temporaire n'a pas pu être supprimé : {file}");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine($"Le fichier temporaire n'a pas pu être supprimé : {file}");
                }
            }
        }
    }
}
