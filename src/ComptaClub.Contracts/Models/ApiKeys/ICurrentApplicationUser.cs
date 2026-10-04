namespace ComptaClub.Contracts.Models.ApiKeys;

public interface ICurrentApplicationUser
{
    Task<Guid?> GetUserId(CancellationToken cancellationToken = default);
}
