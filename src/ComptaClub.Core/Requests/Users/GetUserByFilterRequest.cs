using System.Linq.Expressions;

namespace ComptaClub.Requests.Users;

public record GetUserByFilterRequest : IRequest<UserData?>
{
    public GetUserByFilterRequest(Action<UserListFilter> filter)
    {
        var defaultFilter = new UserListFilter();
        defaultFilter.Options.DeletedState = DeletedState.Undeleted;
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<UserListFilter> Filter { get; init; }
}
