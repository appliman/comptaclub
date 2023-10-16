using Microsoft.JSInterop;

namespace ComptaClub.Blazor.Services;

public class PrintService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<PrintService> _logger;

    public PrintService(IJSRuntime js,
        ILogger<PrintService> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async Task Print(string selector)
    {
        var currentDirectory = System.IO.Directory.GetCurrentDirectory();
        var cssPath = System.IO.Path.Combine(currentDirectory, "wwwroot", "css", "bootstrap", "bootstrap.min.css");
        var bootstrapContent = await System.IO.File.ReadAllTextAsync(cssPath);
        try
        {
            await _js.InvokeVoidAsync("printer.print", selector, bootstrapContent);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while printing");
        }
    }
}
