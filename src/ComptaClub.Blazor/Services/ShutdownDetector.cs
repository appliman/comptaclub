using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComptaClub.Blazor.Services;

public class ShutdownDetector
{
    private readonly WebApplication _app;
	private readonly FileSystemWatcher? _watcher;
    private readonly ILogger _logger;

	public ShutdownDetector(WebApplication app) 
    {
        _app = app;
        _logger = _app.Services.GetRequiredService<ILogger<ShutdownDetector>>();

        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        var currentPath = Path.GetDirectoryName(entryAssembly!.Location)!;
        _watcher = new FileSystemWatcher(currentPath);
        _watcher.Filter = "stop.txt";
        _watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
        _watcher.EnableRaisingEvents = true;
        _watcher.IncludeSubdirectories = false;
        _watcher.Created += OnChanged;
        _watcher.Changed += OnChanged;

        _logger.LogInformation("Shutdown detector initialized. Monitoring path: {Path}", currentPath);
    }

    private async void OnChanged(object source, FileSystemEventArgs e)
    {
        if (_watcher is null)
        {
            // Should not happen, for remove warning
        }

        if (e.Name is null
           || e.Name!.IndexOf("stop", StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            return;
        }

        _logger.LogInformation("Shutdown detector detected stop file creation. Stopping application...");

        await _app.StopAsync();
    }

}
