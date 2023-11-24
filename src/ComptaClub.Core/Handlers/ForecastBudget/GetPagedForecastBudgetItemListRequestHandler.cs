using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.ForecastBudget;

namespace ComptaClub.Handlers.ForecastBudget;

internal class GetPagedForecastBudgetItemListRequestHandler : GetEntityPagedListRequestHandlerBase<ForecastBudgetItemListFilter, ForecastBudgetItemData>
{
    public GetPagedForecastBudgetItemListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<ForecastBudgetItemData>>> Handle(GetPagedEntityListRequest<ForecastBudgetItemListFilter, ForecastBudgetItemData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new ForecastBudgetItemListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from forecastBudgetItem in db.ForecastBudgetItems
                    select forecastBudgetItem;

        if (filter.ForeCastBudgetId.HasValue)
		{
			query = query.Where(i => i.ForecastBudgetId == filter.ForeCastBudgetId.Value);
		}

        var page = await query.GetPagedDataList(i => i.ForecastBudgetId, filter, cancellationToken);

        var result = new PagedList<IEnumerable<ForecastBudgetItemData>>()
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
