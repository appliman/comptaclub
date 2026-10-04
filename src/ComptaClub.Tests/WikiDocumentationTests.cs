using ComptaClub.Blazor.Services.Documentation;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ComptaClub.Tests;

[TestClass]
public class WikiDocumentationTests
{
    [TestMethod]
    public void MarkdownRenderer_ConvertsHeadingsAndLists()
    {
        var _markdown = """
            # Titre Principal
            
            Voici une liste :
            - Elément 1
            - Elément 2
            """;

        var _html = MarkdownRenderer.ToHtml(_markdown);

        _html.Should().Contain("<h1");
        _html.Should().Contain("Titre Principal</h1>");
        _html.Should().Contain("<ul>");
        _html.Should().Contain("<li>Elément 1</li>");
    }

    [TestMethod]
    public void MarkdownRenderer_RewritesRelativeWikiLinks()
    {
        var _markdown = """
            Consultez le [Serveur MCP](Connexion-au-serveur-MCP) ou la [Doc](wiki/Documentation).
            Lien externe : [GitHub](https://github.com/appliman/comptaclub).
            """;

        var _html = MarkdownRenderer.ToHtml(_markdown);

        _html.Should().Contain("href=\"/documentation/Connexion-au-serveur-MCP\"");
        _html.Should().Contain("href=\"/documentation/Documentation\"");
        _html.Should().Contain("href=\"https://github.com/appliman/comptaclub\"");
        _html.Should().Contain("target=\"_blank\"");
    }

    [TestMethod]
    public void MarkdownRenderer_TransformsAlerts()
    {
        var _markdown = """
            > [!IMPORTANT]
            > Copiez la clé immédiatement.

            > [!TIP]
            > Utilisez une variable d'environnement.
            """;

        var _html = MarkdownRenderer.ToHtml(_markdown);

        _html.Should().Contain("alert-primary");
        _html.Should().Contain("Important");
        _html.Should().Contain("Copiez la clé immédiatement.");
        _html.Should().Contain("alert-success");
        _html.Should().Contain("Conseil");
        _html.Should().Contain("Utilisez une variable d'environnement.");
    }

    [TestMethod]
    public void MarkdownRenderer_EnhancesTables()
    {
        var _markdown = """
            | Colonne 1 | Colonne 2 |
            | --- | --- |
            | Valeur A | Valeur B |
            """;

        var _html = MarkdownRenderer.ToHtml(_markdown);

        _html.Should().Contain("table-responsive");
        _html.Should().Contain("table table-striped table-hover table-bordered");
        _html.Should().Contain("Valeur A");
    }

    [TestMethod]
    public void MarkdownRenderer_EnhancesImages()
    {
        var _markdown = """
            ![Capture](images/codex-mcp-configuration.png)
            """;

        var _html = MarkdownRenderer.ToHtml(_markdown);

        _html.Should().Contain("src=\"/images/codex-mcp-configuration.png\"");
        _html.Should().Contain("img-fluid rounded border shadow-sm");
    }

    [TestMethod]
    public async Task WikiDocumentationService_LoadsFallbackWhenOffline()
    {
        var _cache = new MemoryCache(new MemoryCacheOptions());
        var _config = new ConfigurationBuilder().Build();
        var _env = new MockWebHostEnvironment();
        var _clientFactory = new DummyHttpClientFactory();

        var _service = new WikiDocumentationService(
            _clientFactory,
            _cache,
            _env,
            _config,
            NullLogger<WikiDocumentationService>.Instance);

        var _menu = await _service.GetMenuItems();
        _menu.Should().NotBeNull();
        _menu.Should().NotBeEmpty();
        _menu.Should().Contain(m => m.Slug == "Home" || m.Slug == "Connexion-au-serveur-MCP");

        var _page = await _service.GetPage("Connexion-au-serveur-MCP");
        _page.Should().NotBeNull();
        _page!.Html.Should().Contain("MCP");
    }

    private sealed class DummyHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(new ThrowingHttpMessageHandler());

        private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                throw new HttpRequestException("Simulated network outage");
        }
    }

    private sealed class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "ComptaClub.Blazor";
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = default!;
        public string ContentRootPath { get; set; } = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ComptaClub.Blazor"));
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
