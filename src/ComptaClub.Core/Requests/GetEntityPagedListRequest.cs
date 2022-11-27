
namespace ComptaClub.Requests;

public record GetEntityPagedListRequest<F, D> : IRequest<Models.PagedList<IEnumerable<D>>>
    where F : class, IListFilter, new()
    where D : class, Datas.IPrimaryKey, new()
{
    public GetEntityPagedListRequest(Action<F>? filter = null)
    {
        this.Filter = filter;
    }

    public Action<F>? Filter { get; set; }
}
