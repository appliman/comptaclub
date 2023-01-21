using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

namespace ComptaClub.Blazor.Extensions;

public static class ClaimsExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier);
        if (claim == null) 
        {
            return null;
        }
        if (!Guid.TryParse($"{claim.Value}", out var userId))
        {
            return null;
        }
        return userId;
    }
}
