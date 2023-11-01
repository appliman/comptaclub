using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

using Microsoft.AspNetCore.Connections.Features;

namespace ComptaClub.Handlers.Members;

internal class GetPagedMemberListRequestHandler : GetEntityPagedListRequestHandlerBase<MemberListFilter, MemberData>
{
    public GetPagedMemberListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
        : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<MemberData>>> Handle(GetPagedEntityListRequest<MemberListFilter, MemberData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new MemberListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from member in db.Members
                    select member;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search}%";
			query = query.Where(i => EF.Functions.Like(i.Name, pattern)
                                    || EF.Functions.Like(i.LicenseNumber != null ? i.LicenseNumber : "" , pattern)
                                    || EF.Functions.Like(i.Email, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(i => i.Email == filter.Email);
        }

        if (!string.IsNullOrWhiteSpace(filter.LicenseNumber))
        {
            query = query.Where(i => i.LicenseNumber == filter.LicenseNumber);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(i => i.Name == filter.Name);
        }

        if (filter.EntryIdList.Any())
        {
            // On regroupe tous les membres distincts pour la liste des entrées indiquée
            var subQuery = from member in query
                           join mlbe in db.AssociatedMemberListByEntries on member.Id equals mlbe.MemberId
                           join entry in db.Entries on mlbe.EntryId equals entry.Id
                           where filter.EntryIdList.Contains(mlbe.EntryId)
                                && !entry.DeletedDate.HasValue
                           group mlbe by mlbe.MemberId into g
                           select g.Key;

            query = query.Where(i => subQuery.Contains(i.Id));
        }

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

        var result = new PagedList<IEnumerable<MemberData>>()
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
