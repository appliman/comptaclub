using System.Reflection;

using FluentValidation.Results;

namespace ComptaClub.Extensions;

public static class FluentValidationExtensions
{
    public static List<Models.BrokenRule> ToBrokenRules(this List<ValidationFailure> validationFailures)
    {
        if (validationFailures.IsNullOrEmpty())
        {
            return new List<Models.BrokenRule>();
        }

        return validationFailures.GroupBy(x => x.ErrorCode).Select(x => new Models.BrokenRule { PropertyName = x.Key, MessageList = x.Select(g => g.ErrorMessage).ToList() }).ToList();
    }

    public static List<Models.BrokenRule> ToBrokenRules(this ValidationResult validationResult)
    {
        var brokenRules = new List<Models.BrokenRule>();

        if (validationResult == null)
        {
            return brokenRules;
        }

        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors.OrderBy(i => i.PropertyName);

            foreach (var item in validationErrors)
            {
                var severity = Models.Severity.Error;

                switch (item.Severity)
                {
                    case FluentValidation.Severity.Warning:
                        severity = Models.Severity.Warning;
                        break;
                    case FluentValidation.Severity.Info:
                        severity = Models.Severity.Info;
                        break;
                    default:
                        severity = Models.Severity.Error;
                        break;
                }

                var br = brokenRules.FirstOrDefault(i => i.PropertyName == item.PropertyName && i.Severity == severity);

                if (br == null)
                {
                    br = new Models.BrokenRule { PropertyName = item.PropertyName };
                    br.Severity = severity;
                    brokenRules.Add(br);
                }

                br.MessageList.Add(item.ErrorMessage);
            }
        }

        return brokenRules;
    }

    public static Models.PersistResult<T>? ToPersistResult<T>(this List<Models.BrokenRule> brokenRules)
    {
        if (brokenRules.IsNullOrEmpty())
        {
            return null;
        }
        return new Models.PersistResult<T>()
        {
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Error).ToList(),
            WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Warning).ToList(),
            HasError = brokenRules.Count(x => x.Severity == Models.Severity.Error) > 0
        };
    }


    public static List<Models.PersistResult<T>> ToPersistResultList<T>(this List<Models.BrokenRule> brokenRules)
    {
        var result = new List<Models.PersistResult<T>>();

        if (!brokenRules.IsNullOrEmpty())
        {
            result.Add(new Models.PersistResult<T>
            {
                ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Error).ToList(),
                WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Warning).ToList(),
                HasError = brokenRules.Count(x => x.Severity == Models.Severity.Error) > 0
            });
        }

        return result;
    }

	public static Models.PersistResult<T>? ToPersistResult<T>(this ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			return null;
		}
		var brokenRules = validationResult.ToBrokenRules();
		
		return new Models.PersistResult<T>
		{
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Error).ToList(),
			WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Warning).ToList(),
			HasError = brokenRules.Any(x => x.Severity == Models.Severity.Error)
		};
	}

	public static Models.PersistResult? ToPersistResult(this ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			return null;
		}
		var brokenRules = validationResult.ToBrokenRules();

		return new Models.PersistResult
		{
			ErrorBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Error).ToList(),
			WarningBrokenRuleList = brokenRules.Where(i => i.Severity == Models.Severity.Warning).ToList(),
			HasError = brokenRules.Count(x => x.Severity == Models.Severity.Error) > 0
		};
	}

}
