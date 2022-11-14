
namespace ComptaClub.Models;

public class CommandResult
{
    public bool HasError { get; set; }
    public bool HasWarning { get; set; }

    public List<BrokenRule> ErrorBrokenRuleList { get; set; } = new List<BrokenRule>();
    public List<BrokenRule> WarningBrokenRuleList { get; set; } = new List<BrokenRule>();
    public string? ErrorCode { get; set; } 
    public int? ChangeCount { get; set; }

    public void AddErrorBrokenRule(string prop, string msg)
    {
        HasError = true;
        ErrorBrokenRuleList.Add(new BrokenRule(prop, msg));
    }

    public void AddWarningBrokenRule(string prop, string msg)
    {
        HasWarning = true;
        WarningBrokenRuleList.Add(new BrokenRule(prop, msg));
    }

    public string GetAllBrokenRules()
    {
        if (ErrorBrokenRuleList.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (ErrorBrokenRuleList.SelectMany(r => r.MessageList).IsNullOrEmpty())
        {
            return string.Empty;
        }

        return string.Join(',', ErrorBrokenRuleList.Select(r => $"{r.PropertyName}.[{string.Join('|', r.MessageList)}]"));
    }

    public string GetAllWarningBrokenRules()
    {
        if (WarningBrokenRuleList.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (WarningBrokenRuleList.SelectMany(r => r.MessageList).IsNullOrEmpty())
        {
            return string.Empty;
        }

        return string.Join(',', WarningBrokenRuleList.Select(r => $"{r.PropertyName}.[{string.Join('|', r.MessageList)}]"));
    }

    public static CommandResult CreateWarningResult(string warning)
	{
        return new CommandResult
        {
            HasError = false,
            WarningBrokenRuleList = new List<BrokenRule>
            {
                {
                    new BrokenRule
                    {
                        PropertyName = "All",
                        MessageList = new List<string>
                        {
                            warning
                        },
                    }
                }
            }
        };
    }

	public static CommandResult CreateInvalidResult(List<BrokenRule> brokenRuleList)
	{
		return new CommandResult
		{
			HasError = false,
			ErrorBrokenRuleList = new List<BrokenRule>(brokenRuleList)
		};
	}
}

public class PersistResult<T> : CommandResult
{
    public T Id { get; set; } = default(T)!;
    public long AutoInc { get; set; }
    public byte[] Version { get; set; } = null!;
    public string Code { get; set; } = null!;

    public new static PersistResult<T> CreateWarningResult(string warning)
    {
        return new PersistResult<T>
        {
			HasError = false,
			WarningBrokenRuleList = new List<BrokenRule>
            {
                {
                    new BrokenRule
                    {
                        PropertyName = "All",
                        MessageList = new List<string>
                        {
                            warning
                        },
                    }
                }
            }
        };
    }

	public new static PersistResult<T> CreateInvalidResult(List<BrokenRule> brokenRuleList)
	{
		return new PersistResult<T>
		{
			HasError = false,
			ErrorBrokenRuleList = new List<BrokenRule>(brokenRuleList)
		};
	}

    public static PersistResult<T> CreateInvalidResult(string error)
    {
        return new PersistResult<T>
        {
            HasError = false,
            ErrorBrokenRuleList = new List<BrokenRule>()
            {
                {
                    new BrokenRule 
                    {
                        PropertyName = "All",
                        MessageList = new List<string>
                        {
                            error
                        }
                    }
                }
            }
        };
    }

}
