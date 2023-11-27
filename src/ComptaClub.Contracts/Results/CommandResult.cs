namespace ComptaClub.Contracts.Results;

public class CommandResult
{
    public bool HasError { get; set; } = false;
    public bool HasWarning { get; set; } = false;

    public List<BrokenRule> ErrorBrokenRuleList { get; set; } = new();
    public List<BrokenRule> WarningBrokenRuleList { get; set; } = new();
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
        if (ErrorBrokenRuleList is null || ErrorBrokenRuleList.Count == 0)
        {
            return string.Empty;
        }

        var messageList = ErrorBrokenRuleList.SelectMany(r => r.MessageList).ToList();
        if (messageList.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(',', ErrorBrokenRuleList.Select(r => $"{r.PropertyName}.[{string.Join('|', r.MessageList)}]"));
    }

    public string GetAllWarningBrokenRules()
    {
        if (WarningBrokenRuleList is null || WarningBrokenRuleList.Count == 0)
        {
            return string.Empty;
        }

        var warningMessageList = WarningBrokenRuleList.SelectMany(r => r.MessageList).ToList();
        if (warningMessageList.Count == 0)
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
            HasWarning = true,
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
            HasError = true,
            ErrorBrokenRuleList = new List<BrokenRule>(brokenRuleList)
        };
    }

    public static CommandResult CreateInvalidResult(string failReason)
    {
        return new CommandResult
        {
            HasError = true,
            ErrorBrokenRuleList = new List<BrokenRule>
            {
                {
                    new BrokenRule
                    {
                        PropertyName = "All",
                        MessageList = new List<string>
                        {
                            failReason
                        },
                    }
                }
            }
        };
    }

}
