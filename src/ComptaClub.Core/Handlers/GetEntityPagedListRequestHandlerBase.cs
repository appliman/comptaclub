using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal abstract class GetEntityPagedListRequestHandlerBase<F,D> : IRequestHandler<Requests.GetPagedEntityListRequest<F,D>, Models.PagedList<IEnumerable<D>>>
    where F : class, IListFilter, new()
    where D : class, Datas.IPrimaryKey, new()
{
    protected GetEntityPagedListRequestHandlerBase(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        DbContextFactory = dbContextFactory;
    }

    protected IDbContextFactory<ComptaClubDbContext> DbContextFactory { get; }

    public abstract Task<PagedList<IEnumerable<D>>> Handle(GetPagedEntityListRequest<F, D> request, CancellationToken cancellationToken);
}
