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

namespace ComptaClub.Handlers;

internal class ExportPlanToJsonRequestHandler : IRequestHandler<Requests.ExportPlanToJsonFileRequest, CommandResult>
{
    private readonly IMediator _mediator;

    public ExportPlanToJsonRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<CommandResult> Handle(ExportPlanToJsonFileRequest request, CancellationToken cancellationToken)
    {
        var plan = await _mediator.Send(new GetPlanRequest());
        var result = new CommandResult();

        var content = System.Text.Json.JsonSerializer.Serialize(plan, ComptaClub.JsonSerializer.Options);
        try
        {
            if (System.IO.File.Exists(request.FileName))
            {
                System.IO.File.Delete(request.FileName);
            }
            await System.IO.File.WriteAllTextAsync(request.FileName, content, new UTF8Encoding(false));
            result.HasError = false;
        }
        catch(Exception ex) 
        {
            return CommandResult.CreateInvalidResult(ex.Message);
        }

        return result;
    }
}
