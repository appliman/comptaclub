using System.Linq.Expressions;

namespace ComptaClub.Requests.Entries;

public record GetEntryByFilterRequest : IRequest<EntryData?>
{
    public GetEntryByFilterRequest(Action<EntryListFilter> filter)
    {
        var defaultFilter = new EntryListFilter();
        defaultFilter.Options.DeletedState = DeletedState.Undeleted;
        defaultFilter.PageSize = int.MaxValue;
        filter(defaultFilter);
        Filter = filter;
    }

    public Action<EntryListFilter> Filter { get; init; }
}
