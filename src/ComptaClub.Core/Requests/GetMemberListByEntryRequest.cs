namespace ComptaClub.Requests;

public record GetMemberListByEntryRequest : IRequest<IEnumerable<Datas.MemberData>>
{
	public GetMemberListByEntryRequest(Guid entryId)
	{
		this.EntryId = entryId;
	}

	public Guid EntryId { get; init; }
}
