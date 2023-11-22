using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Accounts;

internal class ImportAccountingPlanFromFileStreamRequestHandler : IRequestHandler<ImportAccountingPlanFromFileStreamRequest, CommandResult>
{
	private readonly IMediator _mediator;

	public ImportAccountingPlanFromFileStreamRequestHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task<CommandResult> Handle(ImportAccountingPlanFromFileStreamRequest request, CancellationToken cancellationToken)
	{
		var content = System.Text.Encoding.Default.GetString(request.ContentStream.GetBuffer());

		List<AccountData>? plan = null;
		try
		{
			plan = System.Text.Json.JsonSerializer.Deserialize<List<AccountData>>(content, JsonSerializer.Options);
		}
		catch (Exception ex)
		{
			return CommandResult.CreateInvalidResult(ex.Message);
		}

		var result = await _mediator.Send(new ImportAccountingPlanRequest(plan!), cancellationToken);

		return result;
	}
}

