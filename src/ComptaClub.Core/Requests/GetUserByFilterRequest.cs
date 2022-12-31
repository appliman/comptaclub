using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetUserByFilterRequest : IRequest<Datas.UserData?>
{
    public GetUserByFilterRequest(Expression<Func<Datas.UserData, bool>> filter)
    {
        this.Filter = filter;
    }

    public Expression<Func<Datas.UserData, bool>> Filter { get; init; }
}
