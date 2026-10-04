using ComptaClub.DatabaseConverter;
using ComptaClub.Datas;
using ComptaClub.Datas.MsSql;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ComptaClub.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests;

[TestClass]
public sealed class McpSchemaTests
{
    [TestMethod]
    public async Task Legacy_sqlite_upgrades_without_losing_data_and_conversion_accepts_missing_key_table()
    {
        var _sourceCs = $"Data Source=legacy-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var _targetCs = $"Data Source=target-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var _sourceConnection = new SqliteConnection(_sourceCs);
        await using var _targetConnection = new SqliteConnection(_targetCs);
        await _sourceConnection.OpenAsync();
        await _targetConnection.OpenAsync();
        using var _sourceServices = SqliteServices(_sourceCs);
        using var _targetServices = SqliteServices(_targetCs);
        var _sourceFactory = _sourceServices.GetRequiredService<IComptaClubDbContextFactory>();
        var _targetFactory = _targetServices.GetRequiredService<IComptaClubDbContextFactory>();
        var _userId = Guid.NewGuid();
        await using (var _source = await _sourceFactory.CreateDbContextAsync())
        {
            await _source.GetService<IMigrator>().MigrateAsync(_source.Database.GetMigrations().First());
            _source.Users.Add(new UserData { Id = _userId, Name = "Ancien utilisateur", Email = "legacy@example.org" });
            await _source.SaveChangesAsync();
        }
        await _targetFactory.MigrateAsync();
        var _copy = new DataCopyService();
        await using (var _source = await _sourceFactory.CreateDbContextAsync())
        await using (var _target = await _targetFactory.CreateDbContextAsync())
        {
            var _tables = await _copy.InspectSourceAsync(_source, CancellationToken.None);
            Assert.AreEqual(0L, _tables.Single(item => item.Name == "McpApiKeys").RowCount);
            await _copy.CopyAsync(_source, _target, _tables, false, CancellationToken.None);
            Assert.AreEqual(_userId, (await _target.Users.SingleAsync()).Id);
            Assert.AreEqual(0, await _target.McpApiKeys.CountAsync());
        }
        await _sourceFactory.MigrateAsync();
        await _sourceFactory.MigrateAsync();
        await using var _upgraded = await _sourceFactory.CreateDbContextAsync();
        Assert.AreEqual(_userId, (await _upgraded.Users.SingleAsync()).Id);
        Assert.AreEqual(0, await _upgraded.McpApiKeys.CountAsync());
    }

    [TestMethod]
    public async Task Conversion_preserves_key_hash_lifecycle_and_owner()
    {
        var _sourceCs = $"Data Source=keys-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var _targetCs = $"Data Source=keys-target-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var _sourceConnection = new SqliteConnection(_sourceCs);
        await using var _targetConnection = new SqliteConnection(_targetCs);
        await _sourceConnection.OpenAsync();
        await _targetConnection.OpenAsync();
        using var _sourceServices = SqliteServices(_sourceCs);
        using var _targetServices = SqliteServices(_targetCs);
        var _sourceFactory = _sourceServices.GetRequiredService<IComptaClubDbContextFactory>();
        var _targetFactory = _targetServices.GetRequiredService<IComptaClubDbContextFactory>();
        await _sourceFactory.MigrateAsync();
        await _targetFactory.MigrateAsync();
        var _generated = McpApiKeyGenerator.Generate();
        var _key = new McpApiKeyData
        {
            Id = Guid.NewGuid(), Name = "Codex", KeyIdentifier = _generated.Identifier, SecretHash = _generated.Hash,
            SecretLastFour = _generated.LastFour, CreatedByUserId = Guid.NewGuid(), Version = Guid.NewGuid(),
            CreationDateUtc = DateTime.UtcNow, ExpirationDateUtc = DateTime.UtcNow.AddDays(1),
            RevokedDateUtc = DateTime.UtcNow, ArchivedDateUtc = DateTime.UtcNow, UsageCount = 42
        };
        await using (var _source = await _sourceFactory.CreateDbContextAsync())
        {
            _source.Users.Add(new UserData { Id = _key.CreatedByUserId, Name = "Créateur", Email = "creator@example.org" });
            _source.McpApiKeys.Add(_key);
            await _source.SaveChangesAsync();
        }
        await using (var _source = await _sourceFactory.CreateDbContextAsync())
        await using (var _target = await _targetFactory.CreateDbContextAsync())
        {
            var _copy = new DataCopyService();
            var _tables = await _copy.InspectSourceAsync(_source, CancellationToken.None);
            await _copy.CopyAsync(_source, _target, _tables, false, CancellationToken.None);
            var _stored = await _target.McpApiKeys.SingleAsync();
            Assert.AreEqual(_key.SecretHash, _stored.SecretHash);
            Assert.AreEqual(_key.CreatedByUserId, _stored.CreatedByUserId);
            Assert.AreEqual(_key.Version, _stored.Version);
            Assert.AreEqual(_key.ExpirationDateUtc, _stored.ExpirationDateUtc);
            Assert.AreEqual(_key.RevokedDateUtc, _stored.RevokedDateUtc);
            Assert.AreEqual(_key.ArchivedDateUtc, _stored.ArchivedDateUtc);
            Assert.AreEqual(42L, _stored.UsageCount);
        }
    }

    [TestMethod]
    public void Sql_server_migration_only_creates_mcp_schema()
    {
        var _options = new DbContextOptionsBuilder<McpSchemaDbContext>()
            .UseSqlServer("Server=localhost;Database=DesignTime;Integrated Security=True").Options;
        using var _db = new McpSchemaDbContext(_options);
        var _script = _db.GetService<IMigrator>().GenerateScript();
        StringAssert.Contains(_script, "CREATE TABLE [McpApiKeys]");
        Assert.IsFalse(_script.Contains("CREATE TABLE [Users]", StringComparison.Ordinal));
        Assert.IsFalse(_script.Contains("DROP TABLE", StringComparison.Ordinal));
    }

    [TestMethod]
    [TestCategory("SqlServerIntegration")]
    public async Task Sql_server_legacy_and_mcp_migrations_are_repeatable_on_an_isolated_database()
    {
        var _configured = Environment.GetEnvironmentVariable("COMPTACLUB_MCP_TEST_SQL_SERVER");
        if (string.IsNullOrWhiteSpace(_configured))
        {
            Assert.Inconclusive("Définir COMPTACLUB_MCP_TEST_SQL_SERVER pour tester une base SQL Server temporaire.");
        }
        var _databaseName = $"ComptaClubMcpTest_{Guid.NewGuid():N}";
        var _cs = new SqlConnectionStringBuilder(_configured) { InitialCatalog = _databaseName }.ConnectionString;
        var _services = new ServiceCollection().AddLogging();
        _services.AddComptaClubMsSql(_cs, "Production");
        using var _provider = _services.BuildServiceProvider();
        var _factory = _provider.GetRequiredService<IComptaClubDbContextFactory>();
        var _target = new SqlServerTargetPreparer(_cs);
        try
        {
            await _target.PrepareAsync(CancellationToken.None);
            await _factory.MigrateAsync();
            var _id = Guid.NewGuid();
            await using (var _db = await _factory.CreateDbContextAsync())
            {
                _db.Users.Add(new UserData { Id = _id, Name = "Préservé", Email = "preserved@example.org" });
                await _db.SaveChangesAsync();
            }
            await _factory.MigrateAsync();
            await using var _verified = await _factory.CreateDbContextAsync();
            Assert.AreEqual(_id, (await _verified.Users.SingleAsync()).Id);
            Assert.AreEqual(0, await _verified.McpApiKeys.CountAsync());
        }
        finally
        {
            var _parsed = new SqlConnectionStringBuilder(_cs);
            Assert.AreEqual(_databaseName, _parsed.InitialCatalog);
            Assert.IsTrue(_databaseName.StartsWith("ComptaClubMcpTest_", StringComparison.Ordinal));
            await _target.DropCreatedDatabaseAsync();
        }
    }

    private static ServiceProvider SqliteServices(string connectionString)
    {
        var _services = new ServiceCollection().AddLogging();
        _services.AddComptaClubSqlite(connectionString, "Production");
        return _services.BuildServiceProvider();
    }
}
