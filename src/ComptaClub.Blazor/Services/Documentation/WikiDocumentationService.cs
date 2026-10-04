using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace ComptaClub.Blazor.Services.Documentation;

public sealed partial class WikiDocumentationService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<WikiDocumentationService> logger) : IWikiDocumentationService
{
    private const string MENU_CACHE_KEY = "WIKI_MENU_ITEMS";
    private static readonly TimeSpan DEFAULT_CACHE_DURATION = TimeSpan.FromMinutes(30);

    private string BaseUrl => configuration["Wiki:BaseUrl"] ?? "https://raw.githubusercontent.com/wiki/appliman/comptaclub/";

    public async Task<IReadOnlyList<WikiMenuItem>> GetMenuItems(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(MENU_CACHE_KEY, out IReadOnlyList<WikiMenuItem>? _cached) && _cached is not null)
        {
            return _cached;
        }

        var _items = await FetchMenuItems(cancellationToken);
        if (_items.Count > 0)
        {
            cache.Set(MENU_CACHE_KEY, _items, DEFAULT_CACHE_DURATION);
        }

        return _items;
    }

    public async Task<WikiPageContent?> GetPage(string slug, CancellationToken cancellationToken = default)
    {
        var _normalizedSlug = NormalizeSlug(slug);
        var _cacheKey = $"WIKI_PAGE_{_normalizedSlug.ToUpperInvariant()}";

        if (cache.TryGetValue(_cacheKey, out WikiPageContent? _cached) && _cached is not null)
        {
            return _cached;
        }

        var _page = await FetchPageContent(_normalizedSlug, cancellationToken);
        if (_page is not null)
        {
            cache.Set(_cacheKey, _page, DEFAULT_CACHE_DURATION);
        }

        return _page;
    }

    public void InvalidateCache()
    {
        cache.Remove(MENU_CACHE_KEY);
    }

    private async Task<IReadOnlyList<WikiMenuItem>> FetchMenuItems(CancellationToken cancellationToken)
    {
        var _sidebarContent = await FetchRawString("_Sidebar.md", cancellationToken);

        if (!string.IsNullOrWhiteSpace(_sidebarContent))
        {
            var _parsed = ParseSidebarItems(_sidebarContent);
            if (_parsed.Count > 0)
            {
                return _parsed;
            }
        }

        return GetDefaultMenuItems();
    }

    private static IReadOnlyList<WikiMenuItem> ParseSidebarItems(string content)
    {
        var _results = new List<WikiMenuItem>();
        var _lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var _regex = SidebarItemRegex();

        foreach (var _line in _lines)
        {
            var _match = _regex.Match(_line);
            if (_match.Success)
            {
                var _title = _match.Groups["title"].Value.Trim();
                var _slug = _match.Groups["slug"].Value.Trim();
                var _icon = DetermineIcon(_title, _slug);
                _results.Add(new WikiMenuItem(_title, _slug, _icon));
            }
        }

        return _results;
    }

    private async Task<WikiPageContent?> FetchPageContent(string slug, CancellationToken cancellationToken)
    {
        var _markdown = await FetchRawString($"{slug}.md", cancellationToken);

        if (string.IsNullOrWhiteSpace(_markdown))
        {
            return null;
        }

        var _title = ExtractTitle(_markdown, slug);
        var _html = MarkdownRenderer.ToHtml(_markdown);

        return new WikiPageContent(_title, slug, _markdown, _html);
    }

    private async Task<string?> FetchRawString(string relativeFileName, CancellationToken cancellationToken)
    {
        try
        {
            var _client = httpClientFactory.CreateClient("WikiClient");
            _client.Timeout = TimeSpan.FromSeconds(5);
            var _url = new Uri(new Uri(BaseUrl), relativeFileName).ToString();

            using var _response = await _client.GetAsync(_url, cancellationToken);
            if (_response.IsSuccessStatusCode)
            {
                return await _response.Content.ReadAsStringAsync(cancellationToken);
            }

            logger.LogWarning("Impossible de récupérer la ressource wiki distante {Url} : code {StatusCode}",
                _url, _response.StatusCode);
        }
        catch (Exception _exception) when (_exception is not OperationCanceledException)
        {
            logger.LogWarning(_exception, "Échec de récupération distante du fichier wiki {FileName}, bascule sur le fallback local.",
                relativeFileName);
        }

        return ReadLocalFallback(relativeFileName);
    }

    private string? ReadLocalFallback(string relativeFileName)
    {
        var _candidatePaths = new[]
        {
            Path.Combine(environment.ContentRootPath, "Wiki", relativeFileName),
            Path.Combine(AppContext.BaseDirectory, "Wiki", relativeFileName),
            Path.Combine(environment.ContentRootPath, "..", "..", "docs", "wiki", relativeFileName)
        };

        foreach (var _path in _candidatePaths)
        {
            if (File.Exists(_path))
            {
                try
                {
                    return File.ReadAllText(_path);
                }
                catch (Exception _exception)
                {
                    logger.LogWarning(_exception, "Impossible de lire le fichier de secours local {Path}", _path);
                }
            }
        }

        return null;
    }

    private static string NormalizeSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug == "/" || slug.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            return "Home";
        }

        var _clean = slug.Trim().TrimStart('/');
        if (_clean.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            _clean = _clean[..^3];
        }

        return _clean;
    }

    private static string ExtractTitle(string markdown, string fallbackSlug)
    {
        var _lines = markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var _line in _lines)
        {
            if (_line.StartsWith("# ") && _line.Length > 2)
            {
                return _line[2..].Trim();
            }
        }

        return fallbackSlug.Replace('-', ' ');
    }

    private static string DetermineIcon(string title, string slug)
    {
        var _key = $"{title} {slug}".ToLowerInvariant();
        if (_key.Contains("accueil") || _key.Contains("home"))
        {
            return "fa-house";
        }
        if (_key.Contains("mcp") || _key.Contains("api") || _key.Contains("connexion"))
        {
            return "fa-plug";
        }
        if (_key.Contains("doc"))
        {
            return "fa-book-open";
        }
        return "fa-file-lines";
    }

    private static IReadOnlyList<WikiMenuItem> GetDefaultMenuItems() =>
    [
        new("Accueil", "Home", "fa-house"),
        new("Documentation", "Documentation", "fa-book-open"),
        new("Serveur MCP", "Connexion-au-serveur-MCP", "fa-plug")
    ];

    [GeneratedRegex(@"^[-*]\s+\[(?<title>[^\]]+)\]\((?<slug>[^\)]+)\)", RegexOptions.Compiled)]
    private static partial Regex SidebarItemRegex();
}
