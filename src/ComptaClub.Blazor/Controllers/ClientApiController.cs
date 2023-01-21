using Microsoft.AspNetCore.StaticFiles;

namespace ComptaClub.Blazor.Controllers;

[Microsoft.AspNetCore.Mvc.ApiController]
[Microsoft.AspNetCore.Mvc.Route("/api/client")]
public class ClientApiController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    [Microsoft.AspNetCore.Mvc.HttpGet]
    [Microsoft.AspNetCore.Mvc.Route("dlexport/{*fileName}")]
    public async Task<Microsoft.AspNetCore.Mvc.IActionResult> DownloadExport(string fileName)
    {
        await Task.Delay(1 * 1000);

        var path = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        fileName = System.IO.Path.Combine(path, fileName);
        var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        Response.Headers.Add("Content-Disposition", "plan-comptable.json");
        var result = new Microsoft.AspNetCore.Mvc.FileStreamResult(fs, "application/octet-stream");
        return result;

    }
}
