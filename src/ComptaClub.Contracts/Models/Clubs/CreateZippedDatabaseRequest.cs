namespace ComptaClub.Contracts.Models.Clubs;

public record CreateZippedDatabaseRequest(Guid UserId, string BaseUrl) : IRequest<ZippedDatabaseResult>;
