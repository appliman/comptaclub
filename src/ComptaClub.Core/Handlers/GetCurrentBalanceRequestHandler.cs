using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetCurrentBalanceRequestHandler : IRequestHandler<Requests.GetCurrentBalanceRequest, long>
{
     private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
     private readonly IMediator _mediator;

	public GetCurrentBalanceRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
		MediatR.IMediator mediator)
	{
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
	}

	public async Task<long> Handle(Requests.GetCurrentBalanceRequest request, CancellationToken cancellationToken)
	{
        var db = await _dbContextFactory.CreateDbContextAsync();
		var exercice = await db.Exercices.FirstOrDefaultAsync(i => i.Active);
		if (exercice is null) 
		{
			return 0;
		}

        long globalBalance = exercice.InitialAmount;

        var query = from entry in db.Entries
                    where entry.ValueDate >= exercice.StartDate && entry.ValueDate <= exercice.EndDate
                        && entry.DeletedDate == null
                        && entry.ExerciceId == exercice.Id
                    group entry by new { } into g
                    select new
                    {
                        Balance = g.Sum(i => i.AccountDirection == AccountDirection.Credit ? i.Amount : i.Amount * -1),
                        EntryCount = g.Count()
                    };

        var balanceResult = await query.FirstOrDefaultAsync();
        if (balanceResult != null)
        {
            globalBalance = globalBalance + balanceResult.Balance;
        }

        return globalBalance;
	}
}
