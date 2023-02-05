using ComptaClub.Results;

namespace ComptaClub.Requests;

public record UnlinkMemberToEntryRequest : IRequest<CommandResult>
{
    public UnlinkMemberToEntryRequest(Guid id)
    {
        this.Id = id;   
    }
    public Guid Id { get; init; }
}
