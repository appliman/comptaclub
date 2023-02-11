using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

using Microsoft.Extensions.Options;
using ComptaClub.Requests.Accounts;

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
