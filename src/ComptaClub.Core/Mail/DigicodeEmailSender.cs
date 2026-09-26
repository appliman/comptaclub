using ComptaClub.Configuration;
using HandlebarsDotNet;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ComptaClub.Mail;

public sealed class DigicodeEmailSender(ComptaClubSettings settings)
{
    private static readonly Lazy<HandlebarsTemplate<object, object>> Template = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "Digicode.html");
        return Handlebars.Compile(File.ReadAllText(path));
    });

    public async Task SendAsync(string recipient, int code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || settings.SmtpPort <= 0)
            throw new InvalidOperationException("La configuration SMTP est incomplète.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.ContactName, settings.ContactEmailAdress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = "Votre code d'accès";
        message.Body = new BodyBuilder
        {
            HtmlBody = Template.Value(new { Code = code }),
            TextBody = $"Voici le code pour se connecter : {code}\n\nComptaClub"
        }.ToMessageBody();

        using var client = new SmtpClient();
        var security = settings.SmtpEnableSsl
            ? settings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, security, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.SmtpUserName))
            await client.AuthenticateAsync(settings.SmtpUserName, settings.SmtpPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
