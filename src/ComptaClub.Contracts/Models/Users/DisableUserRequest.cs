using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Users;

public record DisableUserRequest : IRequest<CommandResult>
{
	public DisableUserRequest(Guid userId)
	{
		UserId = userId;
	}

	public Guid UserId { get; set; }
}
