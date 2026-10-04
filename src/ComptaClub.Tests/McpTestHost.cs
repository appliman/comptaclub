using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ComptaClub.Blazor.Services.Mcp;
using ComptaClub.Datas;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ComptaClub.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComptaClub.Tests;

internal sealed class McpTestHost(WebApplication app, HttpClient client, SqliteConnection connection, Guid userId, string secret) : IAsyncDisposable
{
    public WebApplication App { get; } = app;
    public HttpClient Client { get; } = client;
    public Guid UserId { get; } = userId;
    public string Secret { get; } = secret;
    private long _requestId;

    public static async Task<McpTestHost> Create()
    {
        var _builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Test" });
        _builder.Logging.ClearProviders();
        _builder.WebHost.UseTestServer();
        _builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        _builder.Services.AddComptaClubCore();
        var _cs = $"Data Source=mcp-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var _connection = new SqliteConnection(_cs);
        await _connection.OpenAsync();
        _builder.Services.AddComptaClubSqlite(_cs, "Production");
        _builder.Services.AddComptaClubMcp();
        var _app = _builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapComptaClubMcp();
        var _factory = _app.Services.GetRequiredService<IComptaClubDbContextFactory>();
        await _factory.MigrateAsync();
        var _userId = Guid.NewGuid();
        var _generated = McpApiKeyGenerator.Generate();
        await using (var _db = await _factory.CreateDbContextAsync())
        {
            _db.Users.Add(new UserData { Id = _userId, Name = "MCP Test", Email = "mcp@example.org" });
            _db.McpApiKeys.Add(new McpApiKeyData
            {
                Id = Guid.NewGuid(), Name = "Bootstrap", KeyIdentifier = _generated.Identifier,
                SecretHash = _generated.Hash, SecretLastFour = _generated.LastFour,
                CreatedByUserId = _userId, CreationDateUtc = DateTime.UtcNow, Version = Guid.NewGuid()
            });
            await _db.SaveChangesAsync();
        }
        await _app.StartAsync();
        var _client = _app.GetTestClient();
        _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _client.DefaultRequestHeaders.Accept.ParseAdd("text/event-stream");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _generated.PlainText);
        return new McpTestHost(_app, _client, _connection, _userId, _generated.PlainText);
    }

    public async Task<JsonElement> Rpc(string method, object parameters, string? secret = null)
    {
        using var _request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { jsonrpc = "2.0", id = Interlocked.Increment(ref _requestId), method, @params = parameters }), Encoding.UTF8, "application/json")
        };
        if (secret is not null)
        {
            _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
        }
        using var _response = await Client.SendAsync(_request);
        var _body = await _response.Content.ReadAsStringAsync();
        Assert.AreEqual(System.Net.HttpStatusCode.OK, _response.StatusCode, $"RPC {method}: {_response.StatusCode}");
        if (_body.StartsWith("event:", StringComparison.Ordinal) || _body.StartsWith("data:", StringComparison.Ordinal))
        {
            _body = _body.Split('\n').First(line => line.StartsWith("data:", StringComparison.Ordinal))[5..].Trim();
        }
        using var _json = JsonDocument.Parse(_body);
        Assert.IsFalse(_json.RootElement.TryGetProperty("error", out _), $"Erreur protocole pour {method}");
        return _json.RootElement.GetProperty("result").Clone();
    }

    public Task<JsonElement> Tool(string name, object? arguments = null, string? secret = null) =>
        Rpc("tools/call", new { name, arguments = arguments ?? new { } }, secret);

    public static JsonElement Data(JsonElement result)
    {
        Assert.IsFalse(result.TryGetProperty("isError", out var _error) && _error.GetBoolean(),
            result.TryGetProperty("content", out var _content) ? _content.ToString() : "Échec outil");
        return result.GetProperty("structuredContent").GetProperty("data");
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
        await connection.DisposeAsync();
    }
}
