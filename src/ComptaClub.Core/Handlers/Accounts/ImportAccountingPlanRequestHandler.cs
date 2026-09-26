using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Accounts;

internal class ImportAccountingPlanRequestHandler : IRequestHandler<ImportAccountingPlanRequest, CommandResult>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly IMediator _mediator;

	public ImportAccountingPlanRequestHandler(
			IComptaClubDbContextFactory dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<CommandResult> Handle(ImportAccountingPlanRequest request, CancellationToken cancellationToken)
	{
		var list = request.HierarchizedAccountingPlan.ToFlatList();
		var itemCount = 0;
		foreach (var account in list)
		{
			var existing = await _mediator.Send(new GetAccountByFilterRequest(f => f.SetById(account.Id)), cancellationToken);
			if (existing != null)
			{
				continue;
			}
			var saveResult = await _mediator.Send(new SaveEntityRequest<AccountData>(account), cancellationToken);
			if (saveResult.HasError)
			{
				return new CommandResult
				{
					HasError = true,
					ErrorBrokenRuleList = saveResult.ErrorBrokenRuleList
				};
			}
			else
			{
				itemCount++;
			}
		}

		return new CommandResult()
		{
			HasError = false,
			ChangeCount = itemCount
		};
	}


}
