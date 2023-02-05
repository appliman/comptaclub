using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class ImportAccountingPlanRequestHandler : IRequestHandler<Requests.ImportAccountingPlanRequest, Results.CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

	public ImportAccountingPlanRequestHandler(
            IDbContextFactory<ComptaClubDbContext> dbContextFactory,
		MediatR.IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<Results.CommandResult> Handle(Requests.ImportAccountingPlanRequest request, CancellationToken cancellationToken)
	{
		var list = request.HierarchizedAccountingPlan.ToFlatList();
		var itemCount = 0;
		foreach (var account in list)
		{
			var existing = await _mediator.Send(new GetAccountByFilterRequest(f => f.SetById(account.Id)));
			if (existing != null)
			{
				continue;
			}
			var saveResult = await _mediator.Send(new Requests.SaveEntityRequest<Datas.AccountData>(account));
			if (saveResult.HasError)
			{
				return new Results.CommandResult
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

		return new Results.CommandResult()
		{
			HasError = false,
			ChangeCount = itemCount
		};
	}


}
