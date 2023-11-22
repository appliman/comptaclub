
using System.Linq.Expressions;

namespace ComptaClub.Contracts.Models.Accounts;

public record GetAccountByFilterRequest : IRequest<AccountData?>
{
    public GetAccountByFilterRequest(Action<AccountListFilter> filter)
    {
        var defaultFilter = new AccountListFilter();
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<AccountListFilter> Filter { get; init; } = null!;
}
