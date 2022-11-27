namespace ComptaClub.Handlers;

public class GetUserByFilterRequestHandler : IRequestHandler<Requests.GetUserByFilterRequest, Datas.UserData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetUserByFilterRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.UserData?> Handle(Requests.GetUserByFilterRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Users.FirstOrDefaultAsync(request.Filter);

        return data;
    }
}
