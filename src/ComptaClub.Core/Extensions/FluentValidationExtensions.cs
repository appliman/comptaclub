using System.Reflection;
using System.Text;
using ComptaClub.Contracts.Results;
using FluentValidation.Results;

using Microsoft.Extensions.Logging;

namespace ComptaClub.Extensions;

public static class FluentValidationExtensions
{
    public static List<BrokenRule> ToBrokenRules(this List<ValidationFailure> validationFailures)
    {
        if (validationFailures.IsNullOrEmpty())
        {
            return new List<BrokenRule>();
        }

        return validationFailures.GroupBy(x => x.ErrorCode).Select(x => new BrokenRule { PropertyName = x.Key, MessageList = x.Select(g => g.ErrorMessage).ToList() }).ToList();
    }

    public static List<BrokenRule> ToBrokenRules(this ValidationResult validationResult)
    {
        var brokenRules = new List<BrokenRule>();

        if (validationResult == null)
        {
            return brokenRules;
        }

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors.OrderBy(i => i.PropertyName);

            foreach (var item in validationErrors)
            {
                var severity = Contracts.Results.Severity.Error;

                switch (item.Severity)
                {
                    case FluentValidation.Severity.Warning:
                        severity = Contracts.Results.Severity.Warning;
                        break;
                    case FluentValidation.Severity.Info:
                        severity = Contracts.Results.Severity.Info;
                        break;
                    default:
                        severity = Contracts.Results.Severity.Error;
                        break;
                }

                var br = brokenRules.Find(i => i.PropertyName == item.PropertyName && i.Severity == severity);

                if (br == null)
                {
                    br = new BrokenRule { PropertyName = item.PropertyName };
                    br.Severity = severity;
                    brokenRules.Add(br);
                }

                br.MessageList.Add(item.ErrorMessage);
            }
        }

        return brokenRules;
    }

    public static PersistResult? ToPersistResult(this List<BrokenRule> brokenRules)
    {
        if (brokenRules.IsNullOrEmpty())
        {
            return null;
        }
        return new PersistResult()
        {
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Error).ToList(),
            WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Warning).ToList(),
            HasError = brokenRules.Exists(x => x.Severity == Contracts.Results.Severity.Error)
        };
    }
	public static PersistResult? ToPersistResult(this ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			return null;
		}
		var brokenRules = validationResult.ToBrokenRules();

		return new PersistResult
		{
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Error).ToList(),
			WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Warning).ToList(),
			HasError = brokenRules.Exists(x => x.Severity == Contracts.Results.Severity.Error)
		};
	}


	public static List<PersistResult> ToPersistResultList(this List<BrokenRule> brokenRules)
    {
        var result = new List<PersistResult>();

        if (!brokenRules.IsNullOrEmpty())
        {
            result.Add(new PersistResult
            {
                ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Error).ToList(),
                WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Contracts.Results.Severity.Warning).ToList(),
                HasError = brokenRules.Count(x => x.Severity == Contracts.Results.Severity.Error) > 0
            });
        }

        return result;
    }

	public static void LogValidationFailedResult(this ILogger logger, string message, PersistResult validationResult)
	{
		if (validationResult == null)
		{
			return;
		}
		var errorlist = new StringBuilder();
		errorlist.AppendLine(message);
		errorlist.AppendLine($"{validationResult.Id}");
		errorlist.AppendLine($"{validationResult.Code}");
		foreach (var brokenRule in validationResult.ErrorBrokenRuleList)
		{
			errorlist.AppendLine("Property : " + brokenRule.PropertyName);
            foreach (var error in brokenRule.MessageList)
            {
				errorlist.AppendLine("Error Message : " + error);
			}
		}
		logger.LogError(errorlist.ToString());
	}

}
