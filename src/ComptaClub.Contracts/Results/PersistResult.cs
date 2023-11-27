namespace ComptaClub.Contracts.Results;

public class PersistResult : CommandResult
{
    public Guid Id { get; set; }
    public long AutoInc { get; set; }
    public byte[] Version { get; set; } = null!;
    public string Code { get; set; } = null!;

    public new static PersistResult CreateWarningResult(string warning)
    {
        return new PersistResult
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

    public new static PersistResult CreateInvalidResult(List<BrokenRule> brokenRuleList)
    {
        return new PersistResult
        {
            HasError = true,
            ErrorBrokenRuleList = new List<BrokenRule>(brokenRuleList)
        };
    }

    public new static PersistResult CreateInvalidResult(string error)
    {
        return new PersistResult
        {
            HasError = true,
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
