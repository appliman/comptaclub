using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Members;
public record DeleteMemberRequest(Guid MemberId)
    : IRequest<CommandResult>;
