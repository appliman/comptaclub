using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.Stats;

namespace ComptaClub.Handlers.Entries;

internal class ApplyLastEntryForCurrentExerciceNotificationHandler : INotificationHandler<EntrySavedNotification>
{
	private readonly IMediator _mediator;
	private readonly IComptaClubDbContextFactory _dbContextFactory;

	public ApplyLastEntryForCurrentExerciceNotificationHandler(IMediator mediator,
	IComptaClubDbContextFactory dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task Handle(EntrySavedNotification notification, CancellationToken cancellationToken)
	{
		var exercice = await _mediator.Send(new GetExerciceByFilterRequest(i => i.Id == notification.ExerciceId))!;
		if (exercice == null)
		{
			// Ne doit pas arriver
			return;
		}
		exercice!.LastEntryId = notification.EntryId;
		var currentBalance = await _mediator.Send(new GetCurrentBalanceRequest());
		exercice.BalanceAmount = currentBalance;
		await _mediator.Send(new SaveEntityRequest<ExerciceData>(exercice!));
	}
}
