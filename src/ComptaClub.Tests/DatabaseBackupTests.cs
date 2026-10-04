using System.Text.RegularExpressions;
using ChannelMediator;
using ComptaClub.Backups;
using ComptaClub.Configuration;
using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Datas;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace ComptaClub.Tests;

[TestClass]
public sealed class DatabaseBackupTests
{
    [TestMethod]
    public async Task CreatesEncryptedRestorableSnapshotAndEmailsPasswordToRequester()
    {
        var _directory = Path.Combine(Path.GetTempPath(), $"ComptaClubBackupTests-{Guid.NewGuid():N}");
        await using var _app = await CreateApplication(_directory);
        try
        {
            var _factory = _app.Services.GetRequiredService<IComptaClubDbContextFactory>();
            var _userId = Guid.NewGuid();
            await using (var _db = await _factory.CreateDbContextAsync())
            {
                _db.Users.Add(new UserData { Id = _userId, Name = "Demandeur", Email = "requester@example.com" });
                _db.ClubDatas.Add(new ClubData { Id = Guid.NewGuid(), Name = "Club sauvegardé" });
                await _db.SaveChangesAsync();
            }

            var _mediator = _app.Services.GetRequiredService<IMediator>();
            var _result = await _mediator.Send(new CreateZippedDatabaseRequest(_userId, "https://club.example.com/"));
            Assert.IsFalse(_result.HasError, _result.GetAllBrokenRules());
            Assert.IsNotNull(_result.RelativeUrl);
            var _mail = MimeMessage.Load(Directory.GetFiles(Path.Combine(_directory, "Mails"), "*.eml").Single());
            Assert.AreEqual("requester@example.com", _mail.To.Mailboxes.Single().Address);
            Assert.IsNotNull(_mail.TextBody);
            Assert.IsNotNull(_mail.HtmlBody);
            var _password = Regex.Match(_mail.TextBody, "Mot de passe du fichier ZIP : ([A-F0-9]{32})").Groups[1].Value;
            Assert.AreEqual(32, _password.Length);
            StringAssert.Contains(_mail.HtmlBody, _password);
            StringAssert.Contains(_mail.HtmlBody, "https://club.example.com" + _result.RelativeUrl);
            StringAssert.Contains(_mail.TextBody, "24 heures");

            var _store = _app.Services.GetRequiredService<DatabaseArchiveStore>();
            var _token = _result.RelativeUrl.Split('/').Last();
            var _archivePath = _store.Resolve(_token, _userId);
            Assert.IsNotNull(_archivePath);
            Assert.IsNull(_store.Resolve(_token, Guid.NewGuid()));
            Assert.IsNull(_store.Resolve("tampered", _userId));
            Assert.HasCount(0, Directory.GetFiles(Path.Combine(_directory, "DatabaseArchives"), "*.db"));

            using (var _archive = new ZipFile(_archivePath))
            {
                Assert.IsTrue(_archive[0].IsCrypted);
                Assert.AreEqual(256, _archive[0].AESKeySize);
                _archive.Password = "incorrect";
                Assert.Throws<ZipException>(() => _archive.GetInputStream(0));
            }
            var _restoredPath = Path.Combine(_directory, "restored.db");
            using (var _archive = new ZipFile(_archivePath) { Password = _password })
            {
                using var _input = _archive.GetInputStream(0);
                await using var _output = File.Create(_restoredPath);
                await _input.CopyToAsync(_output);
            }
            await using (var _restored = new SqliteConnection($"Data Source={_restoredPath};Pooling=False"))
            {
                await _restored.OpenAsync();
                await using var _command = _restored.CreateCommand();
                _command.CommandText = "SELECT Name FROM Clubs";
                Assert.AreEqual("Club sauvegardé", await _command.ExecuteScalarAsync());
                _command.CommandText = "PRAGMA integrity_check";
                Assert.AreEqual("ok", await _command.ExecuteScalarAsync());
            }

            var _secondResult = await _mediator.Send(new CreateZippedDatabaseRequest(_userId, "https://club.example.com/"));
            Assert.IsFalse(_secondResult.HasError);
            Assert.AreNotEqual(_result.RelativeUrl, _secondResult.RelativeUrl);
            var _protector = _app.Services.GetRequiredService<IDataProtectionProvider>().CreateProtector("ComptaClub.DatabaseArchive.v1");
            var _expired = _protector.Protect($"{Path.GetFileNameWithoutExtension(_archivePath)}|{_userId:N}|{DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds()}");
            Assert.IsNull(_store.Resolve(_expired, _userId));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_directory, true);
        }
    }

    [TestMethod]
    public async Task FailedEmailDoesNotExposeOrRetainArchive()
    {
        var _directory = Path.Combine(Path.GetTempPath(), $"ComptaClubBackupTests-{Guid.NewGuid():N}");
        await using var _app = await CreateApplication(_directory);
        try
        {
            var _settings = _app.Services.GetRequiredService<ComptaClubSettings>();
            _settings.SmtpHost = "";
            var _userId = Guid.NewGuid();
            await using (var _db = await _app.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
            {
                _db.Users.Add(new UserData { Id = _userId, Name = "Demandeur", Email = "requester@example.com" });
                await _db.SaveChangesAsync();
            }
            var _result = await _app.Services.GetRequiredService<IMediator>().Send(new CreateZippedDatabaseRequest(_userId, "https://club.example.com/"));
            Assert.IsTrue(_result.HasError);
            Assert.IsNull(_result.RelativeUrl);
            Assert.HasCount(0, Directory.GetFiles(Path.Combine(_directory, "DatabaseArchives")));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_directory, true);
        }
    }

    [TestMethod]
    public async Task UnknownOrDisabledRequesterCannotCreateArchive()
    {
        var _directory = Path.Combine(Path.GetTempPath(), $"ComptaClubBackupTests-{Guid.NewGuid():N}");
        await using var _app = await CreateApplication(_directory);
        try
        {
            var _mediator = _app.Services.GetRequiredService<IMediator>();
            var _userId = Guid.NewGuid();
            var _result = await _mediator.Send(new CreateZippedDatabaseRequest(_userId, "https://club.example.com/"));
            Assert.IsTrue(_result.HasError);
            await using (var _db = await _app.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
            {
                _db.Users.Add(new UserData { Id = _userId, Name = "Désactivé", Email = "disabled@example.com", DisableDate = 1 });
                await _db.SaveChangesAsync();
            }
            _result = await _mediator.Send(new CreateZippedDatabaseRequest(_userId, "https://club.example.com/"));
            Assert.IsTrue(_result.HasError);
            Assert.IsFalse(Directory.Exists(Path.Combine(_directory, "DatabaseArchives")));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_directory, true);
        }
    }

    private static async Task<WebApplication> CreateApplication(string directory)
    {
        Directory.CreateDirectory(directory);
        var _builder = WebApplication.CreateBuilder();
        _builder.Environment.EnvironmentName = "Test";
        var _settings = new ComptaClubSettings
        {
            DatabaseProvider = "Sqlite",
            ConnectionString = $"Data Source={Path.Combine(directory, "source.db")};Pooling=False",
            TempFolder = directory,
            SmtpHost = "local",
            ContactName = "ComptaClub",
            ContactEmailAdress = "noreply@example.com"
        };
        _builder.Services.AddSingleton(_settings);
        _builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        _builder.Services.AddComptaClubCore();
        _builder.Services.AddComptaClubDatabaseBackups();
        _builder.Services.AddComptaClubSqlite(_settings.ConnectionString, "Test");
        var _app = _builder.Build();
        await _app.Services.GetRequiredService<IComptaClubDbContextFactory>().MigrateAsync();
        return _app;
    }
}
