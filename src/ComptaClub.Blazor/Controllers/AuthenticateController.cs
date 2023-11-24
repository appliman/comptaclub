using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;

namespace ComptaClub.Blazor.Controllers;

public class AuthenticateController : Controller
{
    private readonly IMemoryCache _cache;

    public AuthenticateController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [AllowAnonymous]
    [Microsoft.AspNetCore.Mvc.HttpGet]
    [Microsoft.AspNetCore.Mvc.Route("/authenticate/{token:guid}")]
    public async Task<Microsoft.AspNetCore.Mvc.IActionResult> Authenticate(string token)
    {
        if (User.Identity == null
            || User.Identity!.IsAuthenticated)
        {
            return Redirect("/");
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            return Redirect("/");
        }

        var key = $"login:{token}";

        _cache.TryGetValue(key, out ViewModels.LoginForm? loginForm);
        if (loginForm is null)
        {
            return Redirect("/");
        }

        if (loginForm.ExpirationDate < DateTime.Now)
        {
            _cache.Remove(key);
            return Redirect("/");
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

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            userPrincipal,
            authProperties);

        _cache.Remove(key);

        return Redirect("/");
    }

    [Microsoft.AspNetCore.Mvc.HttpGet]
    [Microsoft.AspNetCore.Mvc.Route("/logout")]
    public async Task<IActionResult> TryLogout()
    {
        if (User.Identity == null
            || !User.Identity.IsAuthenticated)
        {
            return Redirect("/");
        }
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}
