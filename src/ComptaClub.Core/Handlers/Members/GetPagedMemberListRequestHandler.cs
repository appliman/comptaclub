using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Members;

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
									|| EF.Functions.Like(i.LicenseNumber != null ? i.LicenseNumber : "", pattern)
									|| EF.Functions.Like(i.Email, pattern));
		}

		if (!string.IsNullOrWhiteSpace(filter.Email))
		{
            var pattern = $"%{filter.Email}%";
            query = query.Where(i => EF.Functions.Like(i.Email, pattern));
		}

		if (!string.IsNullOrWhiteSpace(filter.LicenseNumber))
		{
            var pattern = $"%{filter.LicenseNumber}%";
            query = query.Where(i => EF.Functions.Like(i.LicenseNumber, pattern));
		}

		if (!string.IsNullOrWhiteSpace(filter.Name))
		{
            var pattern = $"%{filter.Name}%";
            query = query.Where(i => EF.Functions.Like(i.Name, pattern));
		}

		if (!string.IsNullOrWhiteSpace(filter.LicenseTypeName))
		{
			var pattern = $"%{filter.LicenseTypeName}%";
            query = query.Where(i => EF.Functions.Like(i.LicenseTypeName, pattern));
		}

		if (filter.MemberState.HasValue)
        {
            query = query.Where(i => i.State == filter.MemberState.Value);
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
