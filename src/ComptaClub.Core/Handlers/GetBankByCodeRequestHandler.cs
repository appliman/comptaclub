using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetBankByCodeRequestHandler : IRequestHandler<Requests.GetBankByFilterRequest, Datas.BankData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetBankByCodeRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.BankData?> Handle(GetBankByFilterRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Banks.FirstOrDefaultAsync(request.Filter);

        return data;
    }

}
