using ComptaClub.Requests.Banks;

namespace ComptaClub.Handlers.Banks;

internal class GetAllBanksRequestHander : IRequestHandler<GetAllBanksRequest, List<BankData>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAllBanksRequestHander(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<BankData>> Handle(GetAllBanksRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var datas = await db.Banks.ToListAsync();
        return datas;
    }
}

