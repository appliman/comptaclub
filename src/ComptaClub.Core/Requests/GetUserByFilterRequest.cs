using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetUserByFilterRequest : IRequest<Datas.UserData?>
{
    public GetUserByFilterRequest(Action<Models.UserListFilter> filter)
    {
        var defaultFilter = new Models.UserListFilter();
        defaultFilter.Options.DeletedState = DeletedState.Undeleted;
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<Models.UserListFilter> Filter { get; init; }
}
