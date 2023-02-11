using System.Linq.Expressions;

namespace ComptaClub.Requests.Banks;

public record GetBankByFilterRequest : IRequest<BankData?>
{
    public GetBankByFilterRequest(Expression<Func<BankData, bool>> filter)
    {
        Filter = filter;
    }
    public Expression<Func<BankData, bool>> Filter { get; init; } = null!;
}
