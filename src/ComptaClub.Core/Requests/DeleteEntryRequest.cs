using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record DeleteEntryRequest : IRequest<CommandResult>
{
	public DeleteEntryRequest(Guid entryId)
	{
		this.EntryId = entryId;
	}

	public Guid EntryId { get; init; }
}
