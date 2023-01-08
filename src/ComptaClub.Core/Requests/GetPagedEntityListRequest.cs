
namespace ComptaClub.Requests;

public record GetPagedEntityListRequest<F, D> : IRequest<Models.PagedList<IEnumerable<D>>>
    where F : class, IListFilter, new()
    where D : class, Datas.IPrimaryKey, new()
{
    public GetPagedEntityListRequest(Action<F>? filter = null)
    {
        this.Filter = filter;
    }

    public Action<F>? Filter { get; set; }
}
