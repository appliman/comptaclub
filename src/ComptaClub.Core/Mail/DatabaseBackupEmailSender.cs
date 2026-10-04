using ComptaClub.Configuration;
using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ComptaClub.Mail;

public sealed class DatabaseBackupEmailSender(ComptaClubSettings settings)
{
    private static readonly Lazy<HandlebarsTemplate<object, object>> Template = new(() =>
        Handlebars.Compile(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "DatabaseBackup.html"))));

    public async Task SendAsync(string recipient, string password, string downloadUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost)
            || (!settings.SmtpHost.Equals("local", StringComparison.OrdinalIgnoreCase) && settings.SmtpPort <= 0))
        {
            throw new InvalidOperationException("La configuration SMTP est incomplète.");
        }

        var _message = new MimeMessage();
        _message.From.Add(new MailboxAddress(settings.ContactName, settings.ContactEmailAdress));
        _message.To.Add(MailboxAddress.Parse(recipient));
        _message.Subject = "Votre sauvegarde de la base de données ComptaClub";
        _message.Body = new BodyBuilder
        {
            HtmlBody = Template.Value(new { Password = password, DownloadUrl = downloadUrl }),
            TextBody = $"Votre sauvegarde ComptaClub est prête.\n\nTéléchargement : {downloadUrl}\nMot de passe du fichier ZIP : {password}\n\nLe lien est valable 24 heures et nécessite votre connexion à ComptaClub."
        }.ToMessageBody();

        if (settings.SmtpHost.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            var _directory = Path.Combine(settings.TempFolder, "Mails");
            Directory.CreateDirectory(_directory);
            await using var _stream = new FileStream(Path.Combine(_directory, $"{Guid.NewGuid():N}.eml"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
            await _message.WriteToAsync(_stream, cancellationToken);
            return;
        }

        using var _client = new SmtpClient();
        var _security = settings.SmtpEnableSsl
            ? settings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await _client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, _security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.SmtpUserName))
        {
            await _client.AuthenticateAsync(settings.SmtpUserName, settings.SmtpPassword, cancellationToken);
        }
        await _client.SendAsync(_message, cancellationToken);
        await _client.DisconnectAsync(true, cancellationToken);
    }
}
