namespace ComptaClub.Requests;

public record GetAssociatedMemberListByEntryRequest : IRequest<IEnumerable<Datas.AssociatedMemberListByEntryData>>
{
	public GetAssociatedMemberListByEntryRequest(Guid entryId)
	{
		this.EntryId = entryId;
	}

	public Guid EntryId { get; init; }
}
