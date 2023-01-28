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

    public static ViewModels.User? GetUserInfos(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            return null;
        }

        if (!principal.Claims.Any())
        {
            return null;
        }

        var result = new ViewModels.User();
        result.IsAuthenticated = true;              
        var claim = principal.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier);
        if (claim != null
            && Guid.TryParse($"{claim.Value}", out var userId))
        {
            result.Id = userId;
        }
        claim = principal.Claims.SingleOrDefault(i => i.Type == ClaimTypes.Email);
        if (claim != null)
        {
            result.Email = $"{claim.Value}";
        }
        claim = principal.Claims.SingleOrDefault(i => i.Type == ClaimTypes.Name);
        if (claim != null)
        {
            result.Name = $"{claim.Value}";
        }

        return result;
    }

}
