namespace ComptaClub.Requests.Members;

public record GetAssociatedMemberListByEntryRequest : IRequest<IEnumerable<AssociatedMemberListByEntryData>>
{
    public GetAssociatedMemberListByEntryRequest(Guid entryId)
    {
        EntryId = entryId;
    }

    public Guid EntryId { get; init; }
}
