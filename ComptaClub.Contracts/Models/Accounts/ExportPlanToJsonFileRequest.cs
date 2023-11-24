using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Accounts;

public record ExportPlanToJsonFileRequest : IRequest<CommandResult>
{
    public ExportPlanToJsonFileRequest(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; init; }
}
