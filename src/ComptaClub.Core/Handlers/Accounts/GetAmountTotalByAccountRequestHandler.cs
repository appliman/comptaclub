using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Exercices;

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
        Datas.ExerciceData? exercice = null;
        if (request.ExerciceId.HasValue)
        {
            exercice = await _mediator.Send(new GetExerciceByFilterRequest(i => i.Id == request.ExerciceId.Value));
        }
        else
        {
            exercice = await _mediator.Send(new GetActiveExerciceRequest(), cancellationToken);
        }

        if (exercice == null)
        {
            return new List<AmountTotalByAccount>();
        }

        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from entry in db.Entries
                    join account in db.Accounts on entry.AccountId equals account.Id
                    where entry.ValueDate >= exercice.StartDate
                    && entry.ValueDate <= exercice.EndDate
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
                        Label = g.Key.Label,
                        Total = g.Sum(i => i.entry.Amount)
                    };

        var list = await query.ToListAsync(cancellationToken);

        return list;
    }
}
