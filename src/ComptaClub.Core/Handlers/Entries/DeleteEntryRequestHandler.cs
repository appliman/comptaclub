using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Entries;

internal class DeleteEntryRequestHandler : IRequestHandler<DeleteEntryRequest, CommandResult>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly IMediator _mediator;

	public DeleteEntryRequestHandler(IComptaClubDbContextFactory dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<CommandResult> Handle(DeleteEntryRequest request, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var entry = await db.Entries.FindAsync(request.EntryId);
		if (entry == null)
		{
			return CommandResult.CreateWarningResult("Cette ecriture n'existe pas");
		}

		var exercice = await _mediator.Send(new GetActiveExerciceRequest());
		if (exercice != null
			&& entry.ExerciceId != exercice.Id)
		{
			return CommandResult.CreateInvalidResult("Vous ne pouvez pas supprimer une ecriture d'un exercice qui n'est pas actif");
		}

		entry.DeletedDate = DateTime.Today.ToDayId();

		var saveResult = await _mediator.Send(new SaveEntityRequest<EntryData>(entry, true));

		return saveResult;
	}
}
