using ComptaClub.Contracts.Models.ApiKeys;

namespace ComptaClub.Security;

public sealed class AnonymousApplicationUser : ICurrentApplicationUser
{
    public Task<Guid?> GetUserId(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(null);
}
