
using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetAccountByFilterRequest : IRequest<Datas.AccountData?>
{
    public GetAccountByFilterRequest(Expression<Func<Datas.AccountData, bool>> filter)
    {
        this.Filter = filter;   
    }

    public Expression<Func<Datas.AccountData, bool>> Filter { get; init; } = null!;
}
