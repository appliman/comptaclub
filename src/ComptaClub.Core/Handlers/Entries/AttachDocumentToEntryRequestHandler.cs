using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Entries;
internal class AttachDocumentToEntryRequestHandler : IRequestHandler<AttachDocumentToEntryRequest, CommandResult>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly ILogger<AttachDocumentToEntryRequestHandler> _logger;
	private readonly IMediator _mediator;

	public AttachDocumentToEntryRequestHandler(
		IComptaClubDbContextFactory dbContextFactory,
		ILogger<AttachDocumentToEntryRequestHandler> logger,
		IMediator mediator)
	{
		_logger = logger;
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<CommandResult> Handle(AttachDocumentToEntryRequest request, CancellationToken cancellationToken)
	{
		using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var entry = await db.Entries.FindAsync(request.EntryId);
		if (entry == null)
		{
			return CommandResult.CreateInvalidResult("Entry not found");
		}

		var document = await db.Documents.FindAsync(request.Document.Id);
		if (document == null)
		{
			var saveDocument = await _mediator.Send(new SaveDocumentRequest(request.Document), cancellationToken);
			if (saveDocument.HasError)
			{
				return CommandResult.CreateInvalidResult(saveDocument.ErrorBrokenRuleList);
			}
			document = await _mediator.Send(new GetDocumentByFilterRequest(f => f.GetById(saveDocument.Id)), cancellationToken);
		}
		else
		{
			document = request.Document;
		}

		var existingRelationQry = from dbe in db.DocumentsByEntities
								  where dbe.EntityId == request.EntryId
								  && dbe.DocumentId == document!.Id
								  select dbe;

		var relation = await existingRelationQry.FirstOrDefaultAsync(cancellationToken);
		if (relation is null)
		{
			relation = new DocumentByEntityData
			{
				Id = Guid.NewGuid(),
				DocumentId = document!.Id,
				EntityId = request.EntryId,
				MetaEntity = Enums.MetaEntity.Entry,
				CreationDate = DateTime.Now
			};
			db.DocumentsByEntities.Add(relation);
		}

		var changeCount = await db.SaveChangesAsync(cancellationToken);

		return new CommandResult()
		{
			ChangeCount = changeCount
		};
	}
}
