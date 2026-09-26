using System.Text;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Blazor.Extensions;

public static class NotificationServiceExtensions
{
	public static async Task NotifyError(this NotificationService notificationService, CommandResult commandResult)
	{
		var notificationMessage = new NotificationMessage
		{
			Severity = NotificationSeverity.Error,
			Summary = "Erreur lors de la sauvegarde",
			CloseOnClick = true,
			Duration = 10000
		};

		var detail = new StringBuilder();
		foreach (var rule in commandResult.ErrorBrokenRuleList)
		{
			foreach (var error in rule.MessageList)
			{
				detail.AppendLine($"{rule.PropertyName} {error.ToString()}");
			}
		}
		notificationMessage.Detail = detail.ToString();
		await notificationService.Notify(notificationMessage);
	}

	public static async Task NotifyWarning(this NotificationService notificationService, CommandResult commandResult)
	{
		var notificationMessage = new NotificationMessage
		{
			Severity = NotificationSeverity.Warning,
			Summary = "Attention"
		};

		var detail = new StringBuilder();
		foreach (var rule in commandResult.WarningBrokenRuleList)
		{
			foreach (var error in rule.MessageList)
			{
				detail.AppendLine($"{rule.PropertyName} {error.ToString()}");
			}
		}

		notificationMessage.Detail = detail.ToString();
		await notificationService.Notify(notificationMessage);
	}

	public static Task Notify(this NotificationService notificationService, NotificationMessage message) =>
		notificationService.Notify(message.Severity, message.Summary, message.Detail ?? "", message.Duration ?? 3000,
			message.Click, message.CloseOnClick, message.Payload, message.Close, isHtml: false);
}
