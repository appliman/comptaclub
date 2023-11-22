using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Accounts;

internal class DeleteAccountRequestHandler : IRequestHandler<DeleteAccountRequest, CommandResult>
{
	private readonly IMediator _mediator;
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public DeleteAccountRequestHandler(IMediator mediator,
		IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<CommandResult> Handle(DeleteAccountRequest request, CancellationToken cancellationToken)
	{
		// Recherche des entrées associées
		var entriesRequest = new GetPagedEntityListRequest<EntryListFilter, EntryData>(f =>
		{
			f.PageSize = 1;
			f.AccountIdList = new List<Guid> { request.AccountId };
		});
		var entries = await _mediator.Send(entriesRequest, cancellationToken);
		if (entries.List.Any())
		{
			return CommandResult.CreateInvalidResult("Des écritures sont déjà associées à ce compte");
		}

		// Recherche des enfants

		var plan = await _mediator.Send(new GetPlanRequest(), cancellationToken);
		var account = plan.DeepFind(request.AccountId);
		if (account == null)
		{
			return CommandResult.CreateWarningResult("Ce compte n'existe pas");
		}

		if (account.Children.Any())
		{
			return CommandResult.CreateInvalidResult("Ce compte contient d'autres comptes");
		}

		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var data = await db.Accounts.FindAsync(request.AccountId, cancellationToken);
		db.Accounts.Remove(data!);
		db.Entry(data!).State = EntityState.Deleted;

		var changeCount = await db.SaveChangesAsync(cancellationToken);
		return new CommandResult
		{
			ChangeCount = changeCount,
			HasError = changeCount != 1
		};
	}
}
