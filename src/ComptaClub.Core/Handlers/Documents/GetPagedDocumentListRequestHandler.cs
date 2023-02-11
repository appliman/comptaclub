using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers.Documents;

internal class GetPagedDocumentListRequestHandler : GetEntityPagedListRequestHandlerBase<DocumentListFilter, DocumentData>
{
    public GetPagedDocumentListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<DocumentData>>> Handle(GetPagedEntityListRequest<DocumentListFilter, DocumentData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new DocumentListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from document in db.Documents
                    select document;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(i => EF.Functions.Like($"{i.FileName}", $"%{filter.Search}%")
                                    || EF.Functions.Like($"{i.Description}", $"%{filter.Search}%"));
        }

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

        var result = new PagedList<IEnumerable<DocumentData>>()
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
