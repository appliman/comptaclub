
namespace ComptaClub.Results;

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
			HasError = true,
			ErrorBrokenRuleList = new List<BrokenRule>(brokenRuleList)
		};
	}

    public new static PersistResult<T> CreateInvalidResult(string error)
    {
        return new PersistResult<T>
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
