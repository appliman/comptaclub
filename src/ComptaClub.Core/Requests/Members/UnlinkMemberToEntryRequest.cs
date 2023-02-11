using ComptaClub.Results;

namespace ComptaClub.Requests.Members;

public record UnlinkMemberToEntryRequest : IRequest<CommandResult>
{
    public UnlinkMemberToEntryRequest(Guid id)
    {
        Id = id;
    }
    public Guid Id { get; init; }
}
