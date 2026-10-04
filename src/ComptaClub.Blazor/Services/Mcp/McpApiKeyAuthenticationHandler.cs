using System.Security.Claims;
using System.Text.Encodings.Web;
using ComptaClub.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ComptaClub.Blazor.Services.Mcp;

public sealed class McpApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    McpApiKeyService keys) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SCHEME = "McpApiKey";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var _header = Request.Headers["X-Api-Key"];
        var _authorization = Request.Headers.Authorization;
        if (_header.Count > 1 || _authorization.Count > 1)
        {
            return AuthenticateResult.Fail("Identifiants ambigus.");
        }
        var _value = _authorization.ToString();
        var _bearer = _value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? _value[7..].Trim() : null;
        if (_value.Length > 0 && _bearer is null)
        {
            return AuthenticateResult.Fail("Authentification non prise en charge.");
        }
        if (_header.Count == 1 && _bearer is not null && _header[0] != _bearer)
        {
            return AuthenticateResult.Fail("Identifiants contradictoires.");
        }
        var _supplied = _bearer ?? _header.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(_supplied))
        {
            return AuthenticateResult.NoResult();
        }
        var _key = await keys.Validate(_supplied, Context.RequestAborted);
        if (_key is null)
        {
            return AuthenticateResult.Fail("Clé API invalide.");
        }
        var _identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, _key.CreatedByUserId.ToString()),
            new Claim(ClaimTypes.Name, _key.Name),
            new Claim("McpApiKeyId", _key.Id.ToString())
        ], SCHEME);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(_identity), SCHEME));
    }
}
