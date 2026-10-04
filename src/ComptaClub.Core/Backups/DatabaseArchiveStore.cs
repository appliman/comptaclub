using System.Security.Cryptography;
using ComptaClub.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;

namespace ComptaClub.Backups;

public sealed class DatabaseArchiveStore(
    ComptaClubSettings settings,
    IDataProtectionProvider protectionProvider,
    ILogger<DatabaseArchiveStore> logger) : BackgroundService
{
    private readonly IDataProtector _protector = protectionProvider.CreateProtector("ComptaClub.DatabaseArchive.v1");
    private readonly string _directory = Path.GetFullPath(Path.Combine(settings.TempFolder, "DatabaseArchives"));
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public string CreatePath(Guid archiveId)
    {
        return Path.Combine(_directory, $"{archiveId:N}.zip");
    }

    public string CreateDownloadUrl(Guid archiveId, Guid userId)
    {
        var _payload = $"{archiveId:N}|{userId:N}|{DateTimeOffset.UtcNow.Add(Lifetime).ToUnixTimeSeconds()}";
        return $"/api/client/database-archive/{_protector.Protect(_payload)}";
    }

    public string? Resolve(string token, Guid userId)
    {
        try
        {
            var _parts = _protector.Unprotect(token).Split('|');
            if (_parts.Length != 3 || !Guid.TryParseExact(_parts[0], "N", out var _archiveId)
                || !Guid.TryParseExact(_parts[1], "N", out var _ownerId) || _ownerId != userId
                || !long.TryParse(_parts[2], out var _expires) || _expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return null;
            }

            var _path = Path.Combine(_directory, $"{_archiveId:N}.zip");
            return File.Exists(_path) ? _path : null;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _timer = new PeriodicTimer(TimeSpan.FromHours(1));
        do
        {
            try
            {
                if (Directory.Exists(_directory))
                {
                    foreach (var _path in Directory.EnumerateFiles(_directory))
                    {
                        if (File.GetLastWriteTimeUtc(_path) < DateTime.UtcNow.Subtract(Lifetime))
                        {
                            File.Delete(_path);
                        }
                    }
                }
            }
            catch (Exception _exception) when (_exception is IOException or UnauthorizedAccessException)
            {
                logger.LogWarning(_exception, "Impossible de nettoyer les archives expirées de la base de données.");
            }
        }
        while (await _timer.WaitForNextTickAsync(stoppingToken));
    }
}
