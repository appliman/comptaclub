using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.IncomeStatements;

namespace ComptaClub.Handlers.IncomeStatements;

internal class GetPagedIncomeStatementItemListRequestHandler : GetEntityPagedListRequestHandlerBase<IncomeStatementItemListFilter, IncomeStatementItemData>
{
    public GetPagedIncomeStatementItemListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<IncomeStatementItemData>>> Handle(GetPagedEntityListRequest<IncomeStatementItemListFilter, IncomeStatementItemData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new IncomeStatementItemListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from incomeStatementItem in db.IncomeStatementItems
                    select incomeStatementItem;

        if (filter.IncomeStatementId.HasValue)
		{
			query = query.Where(i => i.IncomeStatementId == filter.IncomeStatementId.Value);
		}

        var page = await query.GetPagedDataList(i => i.IncomeStatementId, filter, cancellationToken);

        var result = new PagedList<IEnumerable<IncomeStatementItemData>>()
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
