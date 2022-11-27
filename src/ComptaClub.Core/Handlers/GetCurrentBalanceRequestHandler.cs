using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class GetCurrentBalanceRequestHandler : IRequestHandler<Requests.GetCurrentBalanceRequest, Models.Balance?>
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

	public async Task<Models.Balance?> Handle(Requests.GetCurrentBalanceRequest request, CancellationToken cancellationToken)
	{
        var db = await _dbContextFactory.CreateDbContextAsync();
		var exercice = await db.Exercices.FirstOrDefaultAsync(i => i.Active);
		if (exercice is null) 
		{
			throw new Exception("Il n'y a pas d'exercice en cours");
		}

		if (!exercice.LastEntryId.HasValue)
		{
			return new Balance(exercice.InitialAmount);
		}

		var lastEntry = await _mediator.Send(new GetEntryByFilterRequest(i => i.Id == exercice.LastEntryId.Value));
		if (lastEntry is null)
		{
			throw new Exception("Ne devrait pas arriver");
		}

		return new Balance
		{
			Amount = lastEntry.BalanceValue
        };
	}
}
