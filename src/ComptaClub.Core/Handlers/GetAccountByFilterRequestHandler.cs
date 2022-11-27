namespace ComptaClub.Handlers;

public class GetAccountByFilterRequestHandler : IRequestHandler<Requests.GetAccountByFilterRequest, Datas.AccountData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAccountByFilterRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.AccountData?> Handle(Requests.GetAccountByFilterRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Accounts.FirstOrDefaultAsync(request.Filter);
        return data;
    }
}
