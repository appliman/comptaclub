using ComptaClub.Contracts.Models.Banks;

namespace ComptaClub.Handlers.Banks;

internal class GetAllBanksRequestHander : IRequestHandler<GetAllBanksRequest, List<BankData>>
{
    private readonly IMediator _mediator;
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public GetAllBanksRequestHander(IMediator mediator,
        IComptaClubDbContextFactory dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<BankData>> Handle(GetAllBanksRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var datas = await db.Banks.ToListAsync();
        return datas;
    }
}

