namespace ComptaClub.Handlers;

public class GetEntryByFilterRequestHandler : IRequestHandler<Requests.GetEntryByFilterRequest, Datas.EntryData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetEntryByFilterRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.EntryData?> Handle(Requests.GetEntryByFilterRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Entries.FirstOrDefaultAsync(request.Filter);

        return data;
    }
}
