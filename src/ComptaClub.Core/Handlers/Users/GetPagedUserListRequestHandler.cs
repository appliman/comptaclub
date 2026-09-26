using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Users;

namespace ComptaClub.Handlers.Users;

internal class GetPagedUserListRequestHandler : GetEntityPagedListRequestHandlerBase<UserListFilter, UserData>
{
	public GetPagedUserListRequestHandler(IComptaClubDbContextFactory dbContextFactory)
	: base(dbContextFactory)
	{

	}

	public override async Task<PagedList<IEnumerable<UserData>>> Handle(GetPagedEntityListRequest<UserListFilter, UserData> request, CancellationToken cancellationToken)
	{
		var filter = request.GetFilter(new UserListFilter());

		await using var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

		var query = from user in db.Users
					select user;

		if (!string.IsNullOrWhiteSpace(filter.Search))
		{
			var pattern = $"%{filter.Search}%";
			query = query.Where(i => EF.Functions.Like(i.Name, pattern)
									|| EF.Functions.Like(i.Email, pattern));
		}

		switch (filter.Options.DeletedState)
		{
			case DeletedState.Undeleted:
				query = query.Where(i => i.DisableDate == null);
				break;
			case DeletedState.Delete:
				query = query.Where(i => i.DisableDate != null);
				break;
			case DeletedState.Both:
				break;
			default:
				break;
		}

		if (!string.IsNullOrWhiteSpace(filter.Email))
		{
			query = query.Where(i => i.Email == filter.Email);
		}

		var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

		var result = new PagedList<IEnumerable<UserData>>()
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
