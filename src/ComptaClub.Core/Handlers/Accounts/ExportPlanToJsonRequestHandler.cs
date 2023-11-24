using System.Text;

using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Accounts;

internal class ExportPlanToJsonRequestHandler : IRequestHandler<ExportPlanToJsonFileRequest, CommandResult>
{
	private readonly IMediator _mediator;

	public ExportPlanToJsonRequestHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task<CommandResult> Handle(ExportPlanToJsonFileRequest request, CancellationToken cancellationToken)
	{
		var plan = await _mediator.Send(new GetPlanRequest(), cancellationToken);
		var result = new CommandResult();

		var content = System.Text.Json.JsonSerializer.Serialize(plan, JsonSerializer.Options);
		try
		{
			if (File.Exists(request.FileName))
			{
				File.Delete(request.FileName);
			}
			await File.WriteAllTextAsync(request.FileName, content, new UTF8Encoding(false));
			result.HasError = false;
		}
		catch (Exception ex)
		{
			return CommandResult.CreateInvalidResult(ex.Message);
		}

		return result;
	}
}
