
using Microsoft.AspNetCore.Connections.Features;

namespace ComptaClub.Requests;

public record GetPagedEntityListRequest<F, D> : IRequest<Models.PagedList<IEnumerable<D>>>
    where F : class, IListFilter, new()
    where D : class, Datas.IPrimaryKey, new()
{
    readonly Action<F>? predicateFilter = null;
    readonly F? filter = null;

    public GetPagedEntityListRequest(Action<F>? predicateFilter)
    {
        this.predicateFilter = predicateFilter;
    }

    public GetPagedEntityListRequest(F? filter)
    {
        this.filter = filter;
    }

    public F GetFilter(F inputFilter)
    {
        F result = default(F)!;
        if (filter != null)
        {
            result = this.filter!;
        }
        else if (predicateFilter != null)
        {
            predicateFilter.Invoke(inputFilter);
            result = inputFilter;
        }

        result.EnsureGoodFilter();

        return result;
    }
}
