using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

internal class GetMemberCountRequestHandler : IRequestHandler<GetMemberCountRequest, int>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetMemberCountRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<int> Handle(GetMemberCountRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var result = await db.Members.Where(i => i.State == Datas.Enums.MemberState.Active).CountAsync();

        return result;
    }
}
