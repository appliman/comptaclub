using ComptaClub.Requests.Members;

namespace ComptaClub.Handlers.Members;

internal class GetMemberCountRequestHandler : IRequestHandler<Requests.Members.GetMemberCountRequest, int>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetMemberCountRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<int> Handle(GetMemberCountRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var result = await db.Members.CountAsync();

        return result;
    }
}
