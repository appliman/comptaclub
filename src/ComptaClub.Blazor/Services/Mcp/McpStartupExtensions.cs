using System.Diagnostics;
using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Protocol;

namespace ComptaClub.Blazor.Services.Mcp;

public static class McpStartupExtensions
{
    public static IServiceCollection AddComptaClubMcp(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentApplicationUser, CurrentApplicationUser>();
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, McpApiKeyAuthenticationHandler>(
            McpApiKeyAuthenticationHandler.SCHEME, _ => { });
        services.AddAuthorization();
        services.AddMcpServer().WithHttpTransport(options => options.Stateless = true)
            .WithToolsFromAssembly()
            .WithRequestFilters(filters => filters.AddCallToolFilter(next => async (context, cancellationToken) =>
            {
                var _services = context.Services!;
                var _http = _services.GetRequiredService<IHttpContextAccessor>().HttpContext!;
                var _logger = _services.GetRequiredService<ILoggerFactory>().CreateLogger("ComptaClub.Mcp");
                var _keyId = _http.User.FindFirst("McpApiKeyId")?.Value;
                var _started = Stopwatch.GetTimestamp();
                var _success = false;
                try
                {
                    var _result = await next(context, cancellationToken);
                    _success = _result.IsError != true;
                    return _result;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception _exception)
                {
                    // Tool arguments and exception messages may contain sensitive business data.
                    _logger.LogError("Échec MCP {Tool} pour {ApiKeyId}, type {ExceptionType}.",
                        context.Params?.Name, _keyId, _exception.GetType().Name);
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = [new TextContentBlock { Text = "L’opération a échoué. Consultez les journaux serveur." }]
                    };
                }
                finally
                {
                    _logger.LogInformation("MCP {Tool}, clé {ApiKeyId}, durée {DurationMs} ms, succès {Succeeded}.",
                        context.Params?.Name, _keyId, Stopwatch.GetElapsedTime(_started).TotalMilliseconds, _success);
                    if (Guid.TryParse(_keyId, out var _id))
                    {
                        try
                        {
                            await _services.GetRequiredService<McpApiKeyService>().RegisterUsage(_id, cancellationToken);
                        }
                        catch (Exception _exception) when (_exception is not OperationCanceledException)
                        {
                            _logger.LogError("Impossible de comptabiliser l’appel MCP pour {ApiKeyId}, type {ExceptionType}.",
                                _id, _exception.GetType().Name);
                        }
                    }
                }
            }));
        return services;
    }

    public static IEndpointConventionBuilder MapComptaClubMcp(this WebApplication app) =>
        app.MapMcp("/mcp").RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = McpApiKeyAuthenticationHandler.SCHEME
        });
}
