using System.Security.Claims;
using ComptaClub.Contracts.Models.ApiKeys;

namespace ComptaClub.Blazor.Services.Mcp;

public sealed class CurrentApplicationUser(IHttpContextAccessor httpContextAccessor, IServiceProvider services) : ICurrentApplicationUser
{
    public async Task<Guid?> GetUserId(CancellationToken cancellationToken = default)
    {
        var _principal = httpContextAccessor.HttpContext?.User;
        var _provider = services.GetService<AuthenticationStateProvider>();
        if (_principal?.Identity?.IsAuthenticated != true && _provider is not null)
        {
            _principal = (await _provider.GetAuthenticationStateAsync()).User;
        }
        if (_principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        return Guid.TryParse(_principal.FindFirstValue(ClaimTypes.NameIdentifier), out var _id) ? _id : null;
    }
}
