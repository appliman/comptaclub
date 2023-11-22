using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Members;

public record UnlinkMemberToEntryRequest : IRequest<CommandResult>
{
    public UnlinkMemberToEntryRequest(Guid id)
    {
        Id = id;
    }
    public Guid Id { get; init; }
}
