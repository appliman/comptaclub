using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Entries;
internal class RemoveDocumentFromEntryRequestHandler : IRequestHandler<RemoveDocumentFromEntryRequest, CommandResult>
{
	private readonly IMediator _mediator;
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly ILogger<RemoveDocumentFromEntryRequestHandler> _logger;

	public RemoveDocumentFromEntryRequestHandler(IMediator mediator,
		IComptaClubDbContextFactory dbContextFactory,
		ILogger<RemoveDocumentFromEntryRequestHandler> logger)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
		_logger = logger;
	}

	public async Task<CommandResult> Handle(RemoveDocumentFromEntryRequest request, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var existingRelationQry = from dbe in db.DocumentsByEntities
								  where dbe.EntityId == request.EntryId
									   && dbe.DocumentId == request.DocumentId
									   && dbe.MetaEntity == Enums.MetaEntity.Entry
								  select dbe;

		var dbeList = await existingRelationQry.ToListAsync(cancellationToken);
		foreach (var dbe in dbeList)
		{
			db.DocumentsByEntities.Remove(dbe);
			db.Entry(dbe).State = EntityState.Deleted;
		}

		var changeCount = await db.SaveChangesAsync(cancellationToken);

		return new CommandResult
		{
			ChangeCount = changeCount,
			HasError = false
		};
	}
}