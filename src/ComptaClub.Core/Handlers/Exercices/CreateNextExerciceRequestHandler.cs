using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Exercices;
internal class CreateNextExerciceRequestHandler : IRequestHandler<CreateNextExerciceRequest, ExerciceData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public CreateNextExerciceRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<ExerciceData?> Handle(CreateNextExerciceRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var exercice = await db.Exercices.FindAsync(request.ExerciceId);
		if (exercice == null)
		{
			return null;
		}

		var nextExercice = new ExerciceData
		{
			Id = Guid.NewGuid(),
			CreationDate = DateTime.Today.ToDayId(),
			Code = request.Code,
			Label = request.Label,
			StartDate = exercice.EndDate + 1,
			EndDate = exercice.EndDate + 366,
			InitialAmount = exercice.BalanceAmount,
			BalanceAmount = exercice.BalanceAmount,
			Active = false,
			ExerciceState = Enums.ExerciceState.Current
		};

		return nextExercice;
	}
}