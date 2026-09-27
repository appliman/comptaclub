using ComptaClub.DatabaseConverter;
using ComptaClub.Datas;
using ComptaClub.Datas.MsSql;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ComptaClub.Tests;

[TestClass]
public class DatabaseConverterTests
{
    [TestMethod]
    [TestCategory("SqlServerIntegration")]
    public async Task SqlServer_round_trip_preserves_every_table()
    {
        var serverConnection = Environment.GetEnvironmentVariable("COMPTACLUB_CONVERTER_TEST_SQL_SERVER");
        if (string.IsNullOrWhiteSpace(serverConnection))
        {
            Assert.Inconclusive("Définissez COMPTACLUB_CONVERTER_TEST_SQL_SERVER pour ce test d'intégration.");
        }

        var sqlBuilder = new SqlConnectionStringBuilder(serverConnection)
        {
            InitialCatalog = $"ComptaClubConverterTest_{Guid.NewGuid():N}"
        };
        var target = new SqlServerTargetPreparer(sqlBuilder.ConnectionString);
        var folder = Path.Combine(Path.GetTempPath(), "ComptaClub-ConverterSqlTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var sourcePath = Path.Combine(folder, "source.db");
        var roundTripPath = Path.Combine(folder, "roundtrip.db");
        var bytes = Enumerable.Range(0, 64 * 1024).Select(index => (byte)(index % 251)).ToArray();

        try
        {
            using var sqliteServices = CreateServices(sourcePath);
            var sqliteFactory = sqliteServices.GetRequiredService<IComptaClubDbContextFactory>();
            await sqliteFactory.MigrateAsync();
            await using (var source = await sqliteFactory.CreateDbContextAsync())
            {
                SeedEveryTable(source, Guid.NewGuid(), bytes);
                await source.SaveChangesAsync();
            }

            await target.PrepareAsync(CancellationToken.None);
            using var sqlServices = new ServiceCollection()
                .AddComptaClubMsSql(sqlBuilder.ConnectionString, "Production")
                .BuildServiceProvider();
            var sqlFactory = sqlServices.GetRequiredService<IComptaClubDbContextFactory>();
            var copyService = new DataCopyService();

            IReadOnlyList<TableCopyResult> expected;
            await using (var source = await sqliteFactory.CreateDbContextAsync())
            {
                expected = await copyService.InspectSourceAsync(source, CancellationToken.None);
            }

            Assert.AreEqual(0, await RunConsoleAsync("2", sqlBuilder.ConnectionString, sourcePath));
            await sqlFactory.MigrateAsync();

            await using (var sql = await sqlFactory.CreateDbContextAsync())
            {
                var actual = await copyService.InspectSourceAsync(sql, CancellationToken.None);
                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray());
                CollectionAssert.AreEqual(bytes, (await sql.DocumentsContents.SingleAsync()).Content);
                Assert.AreEqual(42, (await sql.DataProtectionKeys.SingleAsync()).Id);
            }

            Assert.AreEqual(1, await RunConsoleAsync("2", sqlBuilder.ConnectionString, sourcePath));

            await File.WriteAllTextAsync(roundTripPath, "ancienne base");
            Assert.AreEqual(1, await RunConsoleAsync("1", sqlBuilder.ConnectionString, folder, "roundtrip.db", "o", "n"));
            Assert.AreEqual("ancienne base", await File.ReadAllTextAsync(roundTripPath));
            Assert.AreEqual(0, await RunConsoleAsync("1", sqlBuilder.ConnectionString, folder, "roundtrip.db", "o", "o"));

            using var roundTripServices = CreateServices(roundTripPath);
            var roundTripFactory = roundTripServices.GetRequiredService<IComptaClubDbContextFactory>();
            await roundTripFactory.MigrateAsync();

            await using (var roundTrip = await roundTripFactory.CreateDbContextAsync())
            {
                var actual = await copyService.InspectSourceAsync(roundTrip, CancellationToken.None);
                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray());
                CollectionAssert.AreEqual(bytes, (await roundTrip.DocumentsContents.SingleAsync()).Content);
                Assert.IsNull((await roundTrip.ForecastBudgets.SingleAsync()).IncomeStatementId);
            }

            await AssertApplicationStartsAsync("MsSql", sqlBuilder.ConnectionString, null);
            await AssertApplicationStartsAsync("Sqlite", null, roundTripPath);
        }
        finally
        {
            await target.DropCreatedDatabaseAsync();
            Directory.Delete(folder, recursive: true);
        }
    }

    private static async Task<int> RunConsoleAsync(params string[] answers)
    {
        var previousInput = Console.In;
        try
        {
            Console.SetIn(new StringReader(string.Join(Environment.NewLine, answers) + Environment.NewLine));
            return await new ConversionConsole().RunAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetIn(previousInput);
        }
    }

    private static async Task AssertApplicationStartsAsync(string provider, string? sqlConnection, string? sqlitePath)
    {
        var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ComptaClub.Blazor"));
        var applicationDll = Path.Combine(projectDirectory, "bin", "Release", "net10.0", "ComptaClub.Blazor.dll");
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var output = new StringBuilder();
        var start = new ProcessStartInfo("dotnet", $"\"{applicationDll}\"")
        {
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        start.Environment["ComptaClub__DatabaseProvider"] = provider;
        start.Environment["ComptaClub__ConnectionString"] = sqlConnection ?? "";
        start.Environment["ComptaClub__SqliteConnectionString"] = sqlitePath is null ? "" : SqliteFileInspector.CreateWritableConnectionString(sqlitePath);
        start.Environment["ComptaClub__AdminUserEmail"] = "user@example.org";
        start.Environment["ComptaClub__OtlpEndpoint"] = "http://127.0.0.1:4317";
        start.Environment["ComptaClub__AzureStorageConnectionString"] = "UseDevelopmentStorage=true";

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Impossible de démarrer l'application ComptaClub.");
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(args.Data);
            }
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var deadline = DateTime.UtcNow.AddSeconds(25);
            while (DateTime.UtcNow < deadline && !process.HasExited)
            {
                try
                {
                    using var response = await client.GetAsync($"http://127.0.0.1:{port}/");
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }

                await Task.Delay(250);
            }

            var exitCode = process.HasExited ? process.ExitCode.ToString() : "toujours actif";
            var sanitizedOutput = output.ToString();
            if (sqlConnection is not null)
            {
                sanitizedOutput = sanitizedOutput.Replace(sqlConnection, "[chaîne SQL Server masquée]", StringComparison.OrdinalIgnoreCase);
                var password = new SqlConnectionStringBuilder(sqlConnection).Password;
                if (!string.IsNullOrEmpty(password))
                {
                    sanitizedOutput = sanitizedOutput.Replace(password, "[mot de passe masqué]", StringComparison.Ordinal);
                }
            }

            Assert.Fail($"L'application ne démarre pas avec {provider}. Code de sortie : {exitCode}. Sortie : {sanitizedOutput}");
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            await process.WaitForExitAsync();
        }
    }

    [TestMethod]
    public async Task Copy_preserves_every_table_and_binary_content()
    {
        var folder = Path.Combine(Path.GetTempPath(), "ComptaClub-ConverterTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var sourcePath = Path.Combine(folder, "source.db");
        var destinationPath = Path.Combine(folder, "destination.db");
        var bytes = Enumerable.Range(0, 128 * 1024).Select(index => (byte)(index % 251)).ToArray();
        var documentId = Guid.NewGuid();

        try
        {
            using var sourceServices = CreateServices(sourcePath);
            using var destinationServices = CreateServices(destinationPath);
            var sourceFactory = sourceServices.GetRequiredService<IComptaClubDbContextFactory>();
            var destinationFactory = destinationServices.GetRequiredService<IComptaClubDbContextFactory>();
            await sourceFactory.MigrateAsync();
            await destinationFactory.MigrateAsync();

            await using (var source = await sourceFactory.CreateDbContextAsync())
            {
                SeedEveryTable(source, documentId, bytes);
                await source.SaveChangesAsync();
            }

            var copyService = new DataCopyService();
            IReadOnlyList<TableCopyResult> tables;
            await using (var source = await sourceFactory.CreateDbContextAsync())
            {
                tables = await copyService.InspectSourceAsync(source, CancellationToken.None);
                Assert.AreEqual(17, tables.Count);
                Assert.IsTrue(tables.All(table => table.RowCount == 1));
            }

            await using (var source = await sourceFactory.CreateDbContextAsync())
            await using (var destination = await destinationFactory.CreateDbContextAsync())
            {
                await copyService.EnsureDestinationEmptyAsync(destination, CancellationToken.None);
                await copyService.CopyAsync(source, destination, tables, false, CancellationToken.None);
            }

            await using (var destination = await destinationFactory.CreateDbContextAsync())
            {
                var copied = await copyService.InspectSourceAsync(destination, CancellationToken.None);
                CollectionAssert.AreEqual(tables.ToArray(), copied.ToArray());
                CollectionAssert.AreEqual(bytes, (await destination.DocumentsContents.SingleAsync()).Content);
                Assert.AreEqual(42, (await destination.DataProtectionKeys.SingleAsync()).Id);
                Assert.IsNull((await destination.ForecastBudgets.SingleAsync()).IncomeStatementId);
                Assert.AreEqual(new DateTime(2026, 9, 27, 12, 34, 56, DateTimeKind.Utc),
                    (await destination.DocumentsByEntities.SingleAsync()).CreationDate);
                await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                    () => copyService.EnsureDestinationEmptyAsync(destination, CancellationToken.None));
            }

            await SqliteFileInspector.ValidateSourceAsync(destinationPath, CancellationToken.None);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public async Task Sqlite_source_rejects_an_invalid_database_file()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ComptaClub-Invalid-{Guid.NewGuid():N}.db");
        try
        {
            await File.WriteAllTextAsync(path, "Ce fichier n'est pas une base SQLite.");
            await Assert.ThrowsExactlyAsync<Microsoft.Data.Sqlite.SqliteException>(
                () => SqliteFileInspector.ValidateSourceAsync(path, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static ServiceProvider CreateServices(string path)
    {
        var services = new ServiceCollection();
        services.AddComptaClubSqlite(SqliteFileInspector.CreateWritableConnectionString(path), "Production");
        return services.BuildServiceProvider();
    }

    private static void SeedEveryTable(ComptaClubDbContext db, Guid documentId, byte[] content)
    {
        var accountId = Guid.NewGuid();
        var bankId = Guid.NewGuid();
        var exerciceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();

        db.Accounts.Add(new AccountData { Id = accountId, Code = "756", Label = "Cotisations" });
        db.Banks.Add(new BankData { Id = bankId, Code = "B1", Label = "Banque", Active = true });
        db.ClubDatas.Add(new ClubData { Id = Guid.NewGuid(), Name = "Club équestre" });
        db.DataProtectionKeys.Add(new DataProtectionKey { Id = 42, FriendlyName = "test", Xml = "<key />" });
        db.Documents.Add(new DocumentData
        {
            Id = documentId, FileName = "pièce.pdf", MimeType = "application/pdf", Size = content.Length
        });
        db.DocumentsContents.Add(new DocumentContentData { DocumentId = documentId, Content = content });
        db.DocumentsByEntities.Add(new DocumentByEntityData
        {
            Id = Guid.NewGuid(), EntityId = entryId, DocumentId = documentId,
            CreationDate = new DateTime(2026, 9, 27, 12, 34, 56, DateTimeKind.Utc)
        });
        db.Exercices.Add(new ExerciceData { Id = exerciceId, Code = "2026" });
        db.Users.Add(new UserData { Id = userId, Name = "Utilisateur", Email = "user@example.org" });
        db.RolesByUsers.Add(new RoleByUserData { Id = Guid.NewGuid(), UserId = userId, RoleId = Guid.NewGuid() });
        db.Members.Add(new MemberData { Id = memberId, Name = "Membre", Email = "member@example.org" });
        db.Entries.Add(new EntryData
        {
            Id = entryId, PartNumber = "P1", Label = "Cotisation", BankId = bankId,
            AccountId = accountId, ExerciceId = exerciceId, UserCreatorId = userId, Amount = 123456
        });
        db.AssociatedMemberListByEntries.Add(new AssociatedMemberListByEntryData
        {
            Id = Guid.NewGuid(), MemberId = memberId, EntryId = entryId, Amount = 123456
        });
        db.IncomeStatements.Add(new IncomeStatementData
        {
            Id = statementId, ExerciceId = exerciceId, Description = "Résultat"
        });
        db.IncomeStatementItems.Add(new IncomeStatementItemData
        {
            Id = Guid.NewGuid(), IncomeStatementId = statementId, AccountId = accountId,
            Code = "756", Label = "Cotisations"
        });
        db.ForecastBudgets.Add(new ForecastBudgetData
        {
            Id = budgetId, IncomeStatementId = null, Name = "Budget", Description = "Prévision"
        });
        db.ForecastBudgetItems.Add(new ForecastBudgetItemData
        {
            Id = Guid.NewGuid(), ForecastBudgetId = budgetId, AccountId = accountId,
            AccountCode = "756", AccountLabel = "Cotisations"
        });
    }
}
