using ComptaClub.Configuration;
using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Datas;
using ComptaClub.Datas.MsSql;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ComptaClub.Extensions;
using ChannelMediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests;

[TestClass]
public class SqliteProviderTests
{
    [TestMethod]
    public void MsSql_remains_the_default_provider()
    {
        Assert.AreEqual("MsSql", new ComptaClubSettings().DatabaseProvider);
        var services = new ServiceCollection();
        services.AddComptaClubMsSql("Server=localhost;Database=ComptaClubTest;User Id=test;Password=test;TrustServerCertificate=true", "Production");
        using var provider = services.BuildServiceProvider();
        using var db = provider.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContext();
        Assert.AreEqual("Microsoft.EntityFrameworkCore.SqlServer", db.Database.ProviderName);
    }

    [TestMethod]
    public async Task Sqlite_supports_migrations_business_handlers_documents_and_data_protection()
    {
        var folder = Path.Combine(Path.GetTempPath(), "ComptaClub-SqliteTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var connectionString = $"Data Source={Path.Combine(folder, "comptaclub.db")};Pooling=False";

        try
        {
            using var services = CreateServices(connectionString);
            var factory = services.GetRequiredService<IComptaClubDbContextFactory>();
            await factory.MigrateAsync();
            await factory.MigrateAsync();

            var mediator = services.GetRequiredService<IMediator>();
            var user = await mediator.GetOrCreateUser("sqlite@example.com");
            var plan = await mediator.GetOrCreatePlan();
            var bank = await mediator.GetOrCreateBank("SQLite");
            var exercice = await mediator.GetOrCreateExercice("SQLite-2026");
            var account = plan.GetLeafList().Single(i => i.Code == "756001");
            var entry = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, account.Id, new DateTime(2026, 9, 26));
            Assert.IsNotNull(entry);
            var users = await mediator.Send(new GetPagedEntityListRequest<UserListFilter, UserData>(
                new UserListFilter { Search = "sqlite", PageSize = 10 }));
            Assert.AreEqual(1, users.List.Count());
            var budget = await mediator.Send(new CreateAndSaveForecastBudgetRequest("SQLite budget", "Integration test"));
            Assert.IsFalse(budget.HasError);
            var debitAccount = plan.GetLeafList().Single(i => i.Code == "605001");
            var debitEntry = await mediator.CreateAndSaveRandomDebitEntry(exercice, user, bank, debitAccount.Id, new DateTime(2026, 9, 26));
            Assert.IsNotNull(debitEntry);
            var closed = await mediator.Send(new CloseExerciceRequest(exercice.Id));
            Assert.IsFalse(closed.HasError);
            var statement = await mediator.Send(new CreateAndSaveIncomeStatementRequest(exercice.Id));
            Assert.IsFalse(statement.HasError);
            var derivedBudget = await mediator.Send(new CreateAndSaveForecastBudgetFromIncomeStatementRequest(
                statement.Id, "SQLite derived budget", "Integration test"));
            Assert.IsFalse(derivedBudget.HasError);

            var content = "sqlite document content"u8.ToArray();
            var document = await mediator.Send(new CreateDocumentRequest());
            document.FileName = "sqlite.txt";
            document.MimeType = "text/plain";
            using var input = new MemoryStream(content);
            var saved = await mediator.Send(new SaveDocumentRequest(document, input));
            Assert.IsFalse(saved.HasError);
            using var output = new MemoryStream();
            var loaded = await mediator.Send(new GetDocumentContentRequest(saved.Id, output));
            Assert.IsNotNull(loaded);
            CollectionAssert.AreEqual(content, output.ToArray());
            var documents = await mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, DocumentData>(
                new DocumentListFilter { Search = "sqlite", PageSize = 10 }));
            Assert.AreEqual(1, documents.List.Count());

            await using (var db = await factory.CreateDbContextAsync())
            {
                Assert.AreEqual(2, await db.Entries.CountAsync());
                Assert.AreEqual(1, await db.Documents.CountAsync());
                Assert.IsTrue(await db.Accounts.AnyAsync(i => i.Code == "756001"));
                Assert.AreEqual(2, await db.ForecastBudgets.CountAsync());
                Assert.IsTrue(await db.ForecastBudgetItems.AnyAsync(i => i.ForecastBudgetId == budget.Id));
            }

            var protector = services.GetRequiredService<IDataProtectionProvider>().CreateProtector("sqlite-provider-test");
            var protectedValue = protector.Protect("persisted");
            using var restartedServices = CreateServices(connectionString);
            await restartedServices.GetRequiredService<IComptaClubDbContextFactory>().MigrateAsync();
            var restartedProtector = restartedServices.GetRequiredService<IDataProtectionProvider>().CreateProtector("sqlite-provider-test");
            Assert.AreEqual("persisted", restartedProtector.Unprotect(protectedValue));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static ServiceProvider CreateServices(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddComptaClubSqlite(connectionString, "Development");
        services.AddSingleton(new ComptaClubSettings { AdminUserEmail = "sqlite@example.com" });
        services.AddComptaClubCore();
        services.AddDataProtection().SetApplicationName("ComptaClub-SqliteTests")
            .PersistKeysToDbContext<ComptaClubDbContext>();
        return services.BuildServiceProvider();
    }
}
