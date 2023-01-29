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
        var filter = request.GetFilter(new AccountListFilter());

        var db = await DbContextFactory.CreateDbContextAsync();

        var query = from account in db.Accounts
                    select account;

        if (filter.ParentAccountId != null)
        {
            query = query.Where(i => i.ParentAccountId == filter.ParentAccountId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Code))
        {
            query = query.Where(i => i.Code == filter.Code);    
        }

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
