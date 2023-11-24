using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Members;

public record LinkMemberToEntryRequest : IRequest<PersistResult>
{
	public LinkMemberToEntryRequest(Guid entryId, Guid memberId, long amount)
	{
		EntryId = entryId;
		MemberId = memberId;
		Amount = amount;
	}

	public Guid EntryId { get; init; }
	public Guid MemberId { get; init; }
	public long Amount { get; init; }
}
