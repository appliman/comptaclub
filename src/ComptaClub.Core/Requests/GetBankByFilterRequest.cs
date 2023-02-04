using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetBankByFilterRequest : IRequest<Datas.BankData?>
{
    public GetBankByFilterRequest(Expression<Func<Datas.BankData, bool>> filter)
    {
        Filter = filter;
    }
    public Expression<Func<Datas.BankData, bool>> Filter { get; init; } = null!;
}
