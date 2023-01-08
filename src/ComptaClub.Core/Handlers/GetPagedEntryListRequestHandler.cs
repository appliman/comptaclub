using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class GetPagedEntryListRequestHandler : GetEntityPagedListRequestHandlerBase<Models.EntryListFilter, Datas.EntryData>
{
    public GetPagedEntryListRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<EntryData>>> Handle(GetPagedEntityListRequest<EntryListFilter, EntryData> request, CancellationToken cancellationToken)
    {
        var filter = new EntryListFilter();
        request.Filter?.Invoke(filter);

        filter.EnsureGoodFilter();

        var db = await DbContextFactory.CreateDbContextAsync();

        var query = from entry in db.Entries
                    select entry;

        if (filter.AccountIdList.Any())
        {
            query = query.Where(i => filter.AccountIdList.Contains(i.AccountId));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(i => EF.Functions.Like($"{i.ExtraInfos}", $"%{filter.Search}%")
                                    || EF.Functions.Like($"{i.Label}", $"%{filter.Search}%"));
        }

        var page = await query.GetPagedDataList(i => i.CreationDate, filter);

        var result = new PagedList<IEnumerable<EntryData>>()
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
