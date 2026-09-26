using ComptaClub.Contracts.Models.Banks;

namespace ComptaClub.Handlers.Banks;

internal class GetBankByCodeRequestHandler : IRequestHandler<GetBankByFilterRequest, BankData?>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public GetBankByCodeRequestHandler(
        IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<BankData?> Handle(GetBankByFilterRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Banks.FirstOrDefaultAsync(request.Filter);

        return data;
    }

}
