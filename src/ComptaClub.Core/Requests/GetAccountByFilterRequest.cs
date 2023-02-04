
using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetAccountByFilterRequest : IRequest<Datas.AccountData?>
{
    public GetAccountByFilterRequest(Action<AccountListFilter> filter)
    {
        var defaultFilter = new AccountListFilter();
        filter(defaultFilter);
        this.Filter = filter;   
    }

    public Action<AccountListFilter> Filter { get; init; } = null!;
}
