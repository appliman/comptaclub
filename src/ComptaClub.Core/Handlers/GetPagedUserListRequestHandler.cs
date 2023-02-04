using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetPagedUserListRequestHandler : GetEntityPagedListRequestHandlerBase<Models.UserListFilter, Datas.UserData>
{
    public GetPagedUserListRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    : base(dbContextFactory)
    {

    }

    public override async Task<PagedList<IEnumerable<UserData>>> Handle(GetPagedEntityListRequest<UserListFilter, UserData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new UserListFilter());

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from user in db.Users
                    select user;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(i => EF.Functions.Like($"{i.Name}", $"%{filter.Search}%")
                                    || EF.Functions.Like($"{i.Email}", $"%{filter.Search}%"));
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

        var page = await query.GetPagedDataList(i => i.CreationDate, filter);

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
