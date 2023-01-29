using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record LinkMemberToEntryRequest : IRequest<CommandResult>
{
	public LinkMemberToEntryRequest(Guid entryId, Guid memberId)
	{
		this.EntryId = entryId;
		this.MemberId = memberId;
	}

	public Guid EntryId { get; init; }
	public Guid MemberId { get; init; }
}
