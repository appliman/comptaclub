using ComptaClub.Requests;
using ComptaClub.Requests.ForecastBudget;

namespace ComptaClub.Handlers.ForecastBudget;

internal class GetPagedForecastBudgetListRequestHandler : GetEntityPagedListRequestHandlerBase<ForecastBudgetListFilter, ForecastBudgetData>
{
    public GetPagedForecastBudgetListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<ForecastBudgetData>>> Handle(GetPagedEntityListRequest<ForecastBudgetListFilter, ForecastBudgetData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new ForecastBudgetListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from forecastBudget in db.ForecastBudgets
                    select forecastBudget;

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

        var result = new PagedList<IEnumerable<ForecastBudgetData>>()
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
