using System.Security.Cryptography;
using ComptaClub.Backups;
using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Mail;
using ICSharpCode.SharpZipLib.Zip;

namespace ComptaClub.Handlers.Clubs;

internal sealed class CreateZippedDatabaseRequestHandler(
    IComptaClubDbContextFactory dbContextFactory,
    IServiceProvider serviceProvider,
    DatabaseArchiveStore archiveStore,
    DatabaseBackupEmailSender emailSender,
    ILogger<CreateZippedDatabaseRequestHandler> logger) : IRequestHandler<CreateZippedDatabaseRequest, ZippedDatabaseResult>
{
    public async Task<ZippedDatabaseResult> Handle(CreateZippedDatabaseRequest request, CancellationToken cancellationToken)
    {
        var _result = new ZippedDatabaseResult();
        var _backupService = serviceProvider.GetService<IDatabaseBackupService>();
        if (_backupService is null)
        {
            _result.AddErrorBrokenRule("All", "Le téléchargement de la base de données est disponible uniquement avec SQLite.");
            return _result;
        }

        await using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var _user = await _db.Users.SingleOrDefaultAsync(user => user.Id == request.UserId && user.DisableDate == null, cancellationToken);
        if (_user is null || !MimeKit.MailboxAddress.TryParse(_user.Email, out _)
            || !Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out var _baseUri)
            || (_baseUri.Scheme != Uri.UriSchemeHttps && _baseUri.Scheme != Uri.UriSchemeHttp))
        {
            _result.AddErrorBrokenRule("All", "Le demandeur ou son adresse email est invalide.");
            return _result;
        }

        var _archiveId = Guid.NewGuid();
        var _zipPath = archiveStore.CreatePath(_archiveId);
        var _backupPath = Path.ChangeExtension(_zipPath, ".db");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_zipPath)!);
            await _backupService.CreateBackup(_backupPath, cancellationToken);
            var _password = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            await using (var _output = new FileStream(_zipPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            using (var _zip = new ZipOutputStream(_output) { Password = _password, IsStreamOwner = false })
            {
                _zip.SetLevel(6);
                var _entry = new ZipEntry("ComptaClub.db") { AESKeySize = 256, Size = new FileInfo(_backupPath).Length };
                await _zip.PutNextEntryAsync(_entry, cancellationToken);
                await using var _input = new FileStream(_backupPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                await _input.CopyToAsync(_zip, cancellationToken);
                await _zip.CloseEntryAsync(cancellationToken);
                await _zip.FinishAsync(cancellationToken);
            }

            var _relativeUrl = archiveStore.CreateDownloadUrl(_archiveId, _user.Id);
            await emailSender.SendAsync(_user.Email, _password, new Uri(_baseUri, _relativeUrl).AbsoluteUri, cancellationToken);
            _result.RelativeUrl = _relativeUrl;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            DeleteFile(_zipPath);
            throw;
        }
        catch (Exception _exception)
        {
            DeleteFile(_zipPath);
            logger.LogError(_exception, "Échec de la sauvegarde ou de son email pour l’utilisateur {UserId}.", request.UserId);
            _result.AddErrorBrokenRule("All", "Impossible de préparer la sauvegarde et d'envoyer son mot de passe par email. Veuillez réessayer.");
        }
        finally
        {
            DeleteFile(_backupPath);
        }
        return _result;
    }

    private void DeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception _exception) when (_exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(_exception, "Impossible de supprimer un fichier temporaire de sauvegarde.");
        }
    }
}
