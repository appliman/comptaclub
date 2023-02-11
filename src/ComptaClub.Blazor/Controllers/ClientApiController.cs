using ComptaClub.Requests.Documents;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace ComptaClub.Blazor.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
[Microsoft.AspNetCore.Mvc.ApiController]
public class ClientApiController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IMediator _mediator;

    public ClientApiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Microsoft.AspNetCore.Mvc.HttpGet]
    [Microsoft.AspNetCore.Mvc.Route("/api/client/dlexport/{*fileName}")]
    public Microsoft.AspNetCore.Mvc.IActionResult DownloadExport(string fileName)
    {
        var path = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        fileName = System.IO.Path.Combine(path, fileName);
        var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        Response.Headers.Add("Content-Disposition", "attachement; filename=\"plan-comptable.json\"");
        var result = new Microsoft.AspNetCore.Mvc.FileStreamResult(fs, "application/octet-stream");
        return result;

    }

    [Microsoft.AspNetCore.Mvc.HttpGet]
    [Microsoft.AspNetCore.Mvc.Route("document/{documentId:guid}")]
    public async Task<Microsoft.AspNetCore.Mvc.IActionResult> DownloadDocument(Guid documentId)
    {
        var ms = new System.IO.MemoryStream();
        var document = await _mediator.Send(new GetDocumentContentRequest(documentId, ms));
        if (document == null)
        {
            return new EmptyResult();
        }
        Response.Headers.Add("Content-Disposition", $"attachement; filename=\"{document.FileName}\"");
        return File(ms.ToArray(), document.MimeType);
    }

}
