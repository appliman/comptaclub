using System.Security.Claims;
using ComptaClub.Backups;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComptaClub.Blazor.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public sealed class DatabaseArchiveController(DatabaseArchiveStore archiveStore) : ControllerBase
{
    [HttpGet("/api/client/database-archive/{token}")]
    public IActionResult Download(string token)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var _userId))
        {
            return NotFound();
        }
        var _path = archiveStore.Resolve(token, _userId);
        if (_path is null)
        {
            return NotFound();
        }
        Response.Headers.CacheControl = "no-store";
        return PhysicalFile(_path, "application/zip", "Sauvegarde-ComptaClub.zip");
    }
}
