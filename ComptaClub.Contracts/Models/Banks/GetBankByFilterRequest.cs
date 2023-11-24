using System.Linq.Expressions;

namespace ComptaClub.Contracts.Models.Banks;

public record GetBankByFilterRequest : IRequest<BankData?>
{
    public GetBankByFilterRequest(Expression<Func<BankData, bool>> filter)
    {
        Filter = filter;
    }
    public Expression<Func<BankData, bool>> Filter { get; init; } = null!;
}
