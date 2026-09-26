using System.Net;
using System.Net.Sockets;
using System.Text;
using ComptaClub.Configuration;
using ComptaClub.Mail;

namespace ComptaClub.Tests;

[TestClass]
public class DigicodeEmailSenderTests
{
    [TestMethod]
    public async Task Sends_rendered_code_immediately_over_smtp()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var receiveTask = ReceiveMessageAsync(listener, timeout.Token);
            var sender = new DigicodeEmailSender(new ComptaClubSettings
            {
                ContactName = "ComptaClub",
                ContactEmailAdress = "noreply@example.com",
                SmtpHost = "127.0.0.1",
                SmtpPort = port,
                SmtpEnableSsl = false
            });

            await sender.SendAsync("member@example.com", 12345, timeout.Token);
            var message = await receiveTask;

            StringAssert.Contains(message, "member@example.com");
            StringAssert.Contains(message, "12345");
            StringAssert.Contains(message, "text/html");
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task<string> ReceiveMessageAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        using var connection = await listener.AcceptTcpClientAsync(cancellationToken);
        using var stream = connection.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };
        await writer.WriteLineAsync("220 localhost ready");

        var message = new StringBuilder();
        var inData = false;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (inData)
            {
                if (line == ".")
                {
                    inData = false;
                    await writer.WriteLineAsync("250 accepted");
                }
                else
                {
                    message.AppendLine(line);
                }
                continue;
            }

            if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
                await writer.WriteLineAsync("250 localhost");
            else if (line.StartsWith("MAIL FROM", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("RCPT TO", StringComparison.OrdinalIgnoreCase))
                await writer.WriteLineAsync("250 accepted");
            else if (line.Equals("DATA", StringComparison.OrdinalIgnoreCase))
            {
                inData = true;
                await writer.WriteLineAsync("354 send message");
            }
            else if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
            {
                await writer.WriteLineAsync("221 bye");
                break;
            }
            else
                await writer.WriteLineAsync("250 accepted");
        }

        return message.ToString();
    }
}
