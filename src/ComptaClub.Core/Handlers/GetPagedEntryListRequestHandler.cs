using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetPagedEntryListRequestHandler : GetEntityPagedListRequestHandlerBase<Models.EntryListFilter, Datas.EntryData>
{
    public GetPagedEntryListRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<EntryData>>> Handle(GetPagedEntityListRequest<EntryListFilter, EntryData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new EntryListFilter());

        filter.EnsureGoodFilter();

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from entry in db.Entries
                    select entry;

        if (filter.AccountIdList != null
            && filter.AccountIdList.Any())
        {
            query = query.Where(i => filter.AccountIdList.Contains(i.AccountId));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(i => EF.Functions.Like($"{i.ExtraInfos}", $"%{filter.Search}%")
                                    || EF.Functions.Like($"{i.Label}", $"%{filter.Search}%"));
        }

        if (filter.ExerciceId.HasValue)
        {
            query = query.Where(i => i.ExerciceId == filter.ExerciceId.Value);
        }

        if (filter.ImportIdList != null 
            && filter.ImportIdList.Any())
        {
            query = query.Where(i => i.ImportId != null && filter.ImportIdList.Contains(i.ImportId));
        }

        switch (filter.Options.DeletedState)
        {
            case DeletedState.Undeleted:
                query = query.Where(i => i.DeletedDate == null);
                break;
            case DeletedState.Delete:
                query = query.Where(i => i.DeletedDate != null);
                break;
            case DeletedState.Both:
                break;
            default:
                break;
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
