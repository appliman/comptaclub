using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class GetAllBanksRequestHander : IRequestHandler<GetAllBanksRequest, List<Datas.BankData>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAllBanksRequestHander(IMediator mediator,
        IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
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

