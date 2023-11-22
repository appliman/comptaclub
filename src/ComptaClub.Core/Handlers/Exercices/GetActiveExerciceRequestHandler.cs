using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Exercices;

internal class GetActiveExerciceRequestHandler : IRequestHandler<GetActiveExerciceRequest, ExerciceData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public GetActiveExerciceRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<ExerciceData?> Handle(GetActiveExerciceRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var active = await db.Exercices.SingleOrDefaultAsync(i => i.Active, cancellationToken);
		return active;
	}
}
