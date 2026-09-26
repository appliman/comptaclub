using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Entries;
internal class ComputeBalanceForAllEntriesNotificationHandler : INotificationHandler<EntrySavedNotification>
{
	private readonly IMediator _mediator;
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly ILogger<ComputeBalanceForAllEntriesNotificationHandler> _logger;

	public ComputeBalanceForAllEntriesNotificationHandler(IMediator mediator,
		IComptaClubDbContextFactory dbContextFactory,
		ILogger<ComputeBalanceForAllEntriesNotificationHandler> logger)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
		_logger = logger;
	}

	public async Task Handle(EntrySavedNotification notification, CancellationToken cancellationToken)
	{
		// Récupération de l'exercice courant
		var exercice = await _mediator.Send(new GetExerciceByFilterRequest(f => f.Id == notification.ExerciceId), cancellationToken);
		if (exercice is null)
		{
			return;
		}

		// Récupération de toutes les entrées paginées de l'exercice
		var filter = new EntryListFilter();
		filter.PageSize = 100;
		filter.PageIndex = 0;
		filter.ExerciceId = notification.ExerciceId;
		filter.SortByName = "CreationDate";
		filter.SortDirection = System.ComponentModel.ListSortDirection.Ascending;
		filter.ComputeRowCount = ComputeRowCount.Never;
		filter.Options.DeletedState = DeletedState.Undeleted;

		var balance = exercice.InitialAmount;

		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		while (true)
		{
			var page = await _mediator.Send(new GetPagedEntityListRequest<EntryListFilter, EntryData>(filter));
			if (page is null
				|| page.List.IsNullOrEmpty())
			{
				break;
			}
			filter.PageIndex++;

			foreach (var item in page.List)
			{
				balance = balance + (item.Amount * (int)item.AccountDirection);
				if (item.Balance != balance)
				{
					item.Balance = balance;
					db.Entry(item).State = EntityState.Modified;
				}
			}
		}

		using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var changeCount = await db.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			_logger.LogError(ex, ex.Message);
		}
	}
}
