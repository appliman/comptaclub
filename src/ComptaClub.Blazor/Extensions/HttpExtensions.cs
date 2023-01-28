using ComptaClub.Blazor.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace ComptaClub.Blazor.Extensions;

public static class HttpExtensions
{
	public static async Task TrySignIn(this HttpContext ctx, IMemoryCache cache)
	{
		if (ctx.User.Identity == null
			|| ctx.User.Identity!.IsAuthenticated)
		{
			return;
		}
		var tokenParam = $"{ctx.Request.Query["Token"]}";
		if (string.IsNullOrWhiteSpace(tokenParam))
		{
			return;
		}

		var success = Guid.TryParse(tokenParam, out Guid token);
		if (!success
			|| token == Guid.Empty)
		{
			return;
		}

		ctx.Features.TryGetNonEnumeratedCount(out var nonEnumeratedCount);

		var key = $"login:{token}";
        cache.TryGetValue(key, out ViewModels.LoginForm? loginForm);
		if (loginForm == null)
		{
			return;
		}

		if (loginForm.ExpirationDate < DateTime.Now)
		{
			cache.Remove(key);
			return;
		}

		var claims = new List<Claim>();
		claims.Add(new Claim(ClaimTypes.Email, loginForm!.User!.Email));
		claims.Add(new Claim(ClaimTypes.NameIdentifier, $"{loginForm!.User.Id}"));
        claims.Add(new Claim(ClaimTypes.Name, $"{loginForm!.User.Name}"));

        var claimsIdentity = new ClaimsIdentity(
			claims, CookieAuthenticationDefaults.AuthenticationScheme);
		var userPrincipal = new ClaimsPrincipal(claimsIdentity);

		var authProperties = new AuthenticationProperties
		{
			AllowRefresh = true,
			IsPersistent = true,
		};

		await ctx.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			userPrincipal,
			authProperties);

		cache.Remove(key);
    }

	public static async Task TryLogout(this HttpContext ctx)
	{
        if (ctx.User.Identity == null
            || !ctx.User.Identity.IsAuthenticated)
        {
            return;
        }
        var logoffParam = $"{ctx.Request.Query["logout"]}";
        if (string.IsNullOrWhiteSpace(logoffParam))
        {
            return;
        }

        await ctx.SignOutAsync();
	}
}
