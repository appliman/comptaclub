using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record LinkMemberToEntryRequest : IRequest<PersistResult<Guid>>
{
	public LinkMemberToEntryRequest(Guid entryId, Guid memberId, long amount)
	{
		this.EntryId = entryId;
		this.MemberId = memberId;
		this.Amount = amount;
	}

	public Guid EntryId { get; init; }
	public Guid MemberId { get; init; }
	public long Amount { get; init; }
}
