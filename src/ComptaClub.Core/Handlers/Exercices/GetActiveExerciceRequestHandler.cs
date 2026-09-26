using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Exercices;

internal class GetActiveExerciceRequestHandler : IRequestHandler<GetActiveExerciceRequest, ExerciceData?>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;

	public GetActiveExerciceRequestHandler(IComptaClubDbContextFactory dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<ExerciceData?> Handle(GetActiveExerciceRequest request, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var active = await db.Exercices.SingleOrDefaultAsync(i => i.Active, cancellationToken);
		return active;
	}
}
