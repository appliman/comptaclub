using System.Text;

using ComptaClub.Blazor.ViewModels;
using ComptaClub.Results;

using Radzen;

namespace ComptaClub.Blazor.Extensions;

public static class NotificationServiceExtensions
{
    public static void NotifyError(this NotificationService notificationService, CommandResult commandResult)
    {
        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Erreur lors de la sauvegarde"
        };

        var detail = new StringBuilder();
        foreach (var rule in commandResult.ErrorBrokenRuleList)
        {
            foreach (var error in rule.MessageList)
            {
                detail.AppendLine($"{rule.PropertyName} {error.ToString()}");
            }
        }

        notificationService.Notify(notificationMessage);
    }
}
