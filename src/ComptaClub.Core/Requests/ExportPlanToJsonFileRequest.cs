using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record ExportPlanToJsonFileRequest : IRequest<CommandResult>
{
	public ExportPlanToJsonFileRequest(string fileName)
	{
		this.FileName = fileName;
	}

	public string FileName { get; init; }
}
