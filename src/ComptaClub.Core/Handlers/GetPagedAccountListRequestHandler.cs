using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetPagedAccountListRequestHandler : GetEntityPagedListRequestHandlerBase<Models.AccountListFilter, Datas.AccountData>
{
    public GetPagedAccountListRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<AccountData>>> Handle(GetPagedEntityListRequest<AccountListFilter, AccountData> request, CancellationToken cancellationToken)
    {
        var filter = new AccountListFilter();
        request.Filter?.Invoke(filter);

        filter.EnsureGoodFilter();

        var db = await DbContextFactory.CreateDbContextAsync();

        var query = from account in db.Accounts
                    select account;

        var page = await query.GetPagedDataList(i => i.CreationDate, filter);

        var result = new PagedList<IEnumerable<AccountData>>()
        {
            List = page.List,
            Total = new PagedTotal
            {
                RowCount = page.Count
            }
        };

        return result;
    }
}
