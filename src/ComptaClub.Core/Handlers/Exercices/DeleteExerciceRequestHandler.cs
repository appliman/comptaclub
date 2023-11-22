using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Exercices;

internal class DeleteExerciceRequestHandler : IRequestHandler<DeleteExerciceRequest, CommandResult>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public DeleteExerciceRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<CommandResult> Handle(DeleteExerciceRequest request, CancellationToken cancellationToken)
	{
		// On regarde s'il existe déjà des entrées associées à l'exercice
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var exercice = await db.Exercices.FindAsync(request.ExerciceId);

		if (exercice == null)
		{
			return CommandResult.CreateWarningResult("Cet exercice n'existe pas");
		}

		if (exercice.ClosedDate.HasValue)
		{
			return CommandResult.CreateInvalidResult("Il n'est pas possible de supprimer un exercice clos");
		}

		var entryCount = await db.Entries.CountAsync(i => i.ExerciceId == request.ExerciceId);
		if (entryCount > 0)
		{
			return CommandResult.CreateInvalidResult("Il n'est pas possible de supprimer un exercice avec des ecritures");
		}

		db.Exercices.Remove(exercice);
		db.Entry(exercice).State = EntityState.Deleted;

		var changeCount = await db.SaveChangesAsync(cancellationToken);

		return new CommandResult
		{
			ChangeCount = changeCount,
			HasError = false
		};
	}
}
