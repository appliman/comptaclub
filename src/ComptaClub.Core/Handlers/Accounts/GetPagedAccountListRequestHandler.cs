using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class GetPagedAccountListRequestHandler : GetEntityPagedListRequestHandlerBase<AccountListFilter, AccountData>
{
    public GetPagedAccountListRequestHandler(IComptaClubDbContextFactory dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<AccountData>>> Handle(GetPagedEntityListRequest<AccountListFilter, AccountData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new AccountListFilter());

        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

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

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var _pattern = $"%{filter.Search}%";
            query = query.Where(item => EF.Functions.Like(item.Code, _pattern) || EF.Functions.Like(item.Label, _pattern));
        }

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

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
