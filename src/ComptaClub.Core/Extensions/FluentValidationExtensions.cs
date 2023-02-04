using System.Reflection;
using System.Text;

using FluentValidation.Results;

using Microsoft.Extensions.Logging;

namespace ComptaClub.Extensions;

public static class FluentValidationExtensions
{
    public static List<Results.BrokenRule> ToBrokenRules(this List<ValidationFailure> validationFailures)
    {
        if (validationFailures.IsNullOrEmpty())
        {
            return new List<Results.BrokenRule>();
        }

        return validationFailures.GroupBy(x => x.ErrorCode).Select(x => new Results.BrokenRule { PropertyName = x.Key, MessageList = x.Select(g => g.ErrorMessage).ToList() }).ToList();
    }

    public static List<Results.BrokenRule> ToBrokenRules(this ValidationResult validationResult)
    {
        var brokenRules = new List<Results.BrokenRule>();

        if (validationResult == null)
        {
            return brokenRules;
        }

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors.OrderBy(i => i.PropertyName);

            foreach (var item in validationErrors)
            {
                var severity = Results.Severity.Error;

                switch (item.Severity)
                {
                    case FluentValidation.Severity.Warning:
                        severity = Results.Severity.Warning;
                        break;
                    case FluentValidation.Severity.Info:
                        severity = Results.Severity.Info;
                        break;
                    default:
                        severity = Results.Severity.Error;
                        break;
                }

                var br = brokenRules.FirstOrDefault(i => i.PropertyName == item.PropertyName && i.Severity == severity);

                if (br == null)
                {
                    br = new Results.BrokenRule { PropertyName = item.PropertyName };
                    br.Severity = severity;
                    brokenRules.Add(br);
                }

                br.MessageList.Add(item.ErrorMessage);
            }
        }

        return brokenRules;
    }

    public static Results.PersistResult<T>? ToPersistResult<T>(this List<Results.BrokenRule> brokenRules)
    {
        if (brokenRules.IsNullOrEmpty())
        {
            return null;
        }
        return new Results.PersistResult<T>()
        {
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Error).ToList(),
            WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Warning).ToList(),
            HasError = brokenRules.Count(x => x.Severity == Results.Severity.Error) > 0
        };
    }


    public static List<Results.PersistResult<T>> ToPersistResultList<T>(this List<Results.BrokenRule> brokenRules)
    {
        var result = new List<Results.PersistResult<T>>();

        if (!brokenRules.IsNullOrEmpty())
        {
            result.Add(new Results.PersistResult<T>
            {
                ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Error).ToList(),
                WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Warning).ToList(),
                HasError = brokenRules.Count(x => x.Severity == Results.Severity.Error) > 0
            });
        }

        return result;
    }

	public static Results.PersistResult<T>? ToPersistResult<T>(this ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			return null;
		}
		var brokenRules = validationResult.ToBrokenRules();
		
		return new Results.PersistResult<T>
		{
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Error).ToList(),
			WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Warning).ToList(),
			HasError = brokenRules.Any(x => x.Severity == Results.Severity.Error)
		};
	}

	public static Results.CommandResult? ToPersistResult(this ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			return null;
		}
		var brokenRules = validationResult.ToBrokenRules();

		return new Results.CommandResult
		{
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Error).ToList(),
			WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Results.Severity.Warning).ToList(),
			HasError = brokenRules.Count(x => x.Severity == Results.Severity.Error) > 0
		};
	}

	public static void LogValidationFailedResult<T>(this ILogger logger, string message, Results.PersistResult<T> validationResult)
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
