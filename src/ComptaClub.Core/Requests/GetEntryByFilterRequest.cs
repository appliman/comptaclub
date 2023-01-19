using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetEntryByFilterRequest : IRequest<Datas.EntryData?>
{
    public GetEntryByFilterRequest(Action<Models.EntryListFilter> filter)
    {
        var defaultFilter = new Models.EntryListFilter();
        defaultFilter.Options.DeletedState = DeletedState.Undeleted;
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<Models.EntryListFilter> Filter { get; init; }
}
