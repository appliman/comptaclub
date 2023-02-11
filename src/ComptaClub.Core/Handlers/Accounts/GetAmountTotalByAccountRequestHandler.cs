using ComptaClub.Requests.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class GetAmountTotalByAccountRequestHandler : IRequestHandler<GetAmountTotalByAccountRequest, IEnumerable<AmountTotalByAccount>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAmountTotalByAccountRequestHandler(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<AmountTotalByAccount>> Handle(GetAmountTotalByAccountRequest request, CancellationToken cancellationToken)
    {
        var currentExercice = await _mediator.Send(new Requests.Exercices.GetActiveExerciceRequest(), cancellationToken);
        if (currentExercice == null)
        {
            return new List<AmountTotalByAccount>();
        }

        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from entry in db.Entries
                    join account in db.Accounts on entry.AccountId equals account.Id
                    where entry.ValueDate >= currentExercice.StartDate
                    && entry.ValueDate <= currentExercice.EndDate
                    && entry.DeletedDate == null
                    group new
                    {
                        entry.AccountId,
                        entry,
                        account
                    } by new { entry.AccountId, account.Code, account.Label } into g
                    select new AmountTotalByAccount
                    {
                        Id = g.Key.AccountId,
                        Code = g.Key.Code,
                        Lablel = g.Key.Label,
                        Total = g.Sum(i => i.entry.Amount) / 1000000m
                    };

        var list = await query.ToListAsync(cancellationToken);

        return list;
    }
}
