using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetEntryByFilterRequest : IRequest<Datas.EntryData?>
{
    public GetEntryByFilterRequest(Expression<Func<Datas.EntryData, bool>> filter)
    {
        this.Filter = filter;
    }

    public Expression<Func<Datas.EntryData, bool>> Filter { get; init; }
}
