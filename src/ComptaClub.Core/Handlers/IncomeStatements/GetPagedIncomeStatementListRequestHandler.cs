using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.IncomeStatements;

namespace ComptaClub.Handlers.IncomeStatements;

internal class GetPagedIncomeStatementListRequestHandler : GetEntityPagedListRequestHandlerBase<IncomeStatementListFilter, IncomeStatementData>
{
    public GetPagedIncomeStatementListRequestHandler(IComptaClubDbContextFactory dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<IncomeStatementData>>> Handle(GetPagedEntityListRequest<IncomeStatementListFilter, IncomeStatementData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new IncomeStatementListFilter());

        await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from incomeStatement in db.IncomeStatements
                    select incomeStatement;

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

        var result = new PagedList<IEnumerable<IncomeStatementData>>()
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
