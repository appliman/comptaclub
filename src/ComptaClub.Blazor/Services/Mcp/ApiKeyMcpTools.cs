using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ChannelMediator;
using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Models.Stats;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ComptaClub.Blazor.Services.Mcp;

[McpServerToolType]
public sealed class ApiKeyMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "list_api_keys", ReadOnly = true, Destructive = false)]
    [Description("Liste les clés MCP masquées.")]
    public Task<CallToolResult> ListKeys(int pageIndex = 0, int pageSize = 50, string? search = null, bool includeArchived = false, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new ListMcpApiKeysRequest(pageIndex, pageSize, search, includeArchived), cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_api_key", ReadOnly = true, Destructive = false)]
    [Description("Retourne les métadonnées masquées et la version d’une clé MCP.")]
    public Task<CallToolResult> GetKey(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetMcpApiKeyRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_api_key", ReadOnly = false, Destructive = false)]
    [Description("Crée une clé MCP complète. Le secret est retourné une seule fois ; expiration UTC facultative.")]
    public Task<CallToolResult> CreateKey(string name, DateTime? expirationDateUtc = null, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new CreateMcpApiKeyRequest(name, expirationDateUtc), cancellationToken), cancellationToken);

    [McpServerTool(Name = "update_api_key", ReadOnly = false, Destructive = false)]
    [Description("Modifie le nom et l’expiration d’une clé avec contrôle de version.")]
    public Task<CallToolResult> UpdateKey(Guid id, Guid expectedVersion, string name, DateTime? expirationDateUtc = null, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new UpdateMcpApiKeyRequest(id, expectedVersion, name, expirationDateUtc), cancellationToken), cancellationToken);

    [McpServerTool(Name = "rotate_api_key", ReadOnly = false, Destructive = true)]
    [Description("Remplace immédiatement une clé ; retourne le nouveau secret une seule fois.")]
    public Task<CallToolResult> RotateKey(Guid id, Guid expectedVersion, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new RotateMcpApiKeyRequest(id, expectedVersion), cancellationToken), cancellationToken);

    [McpServerTool(Name = "revoke_api_key", ReadOnly = false, Destructive = true)]
    [Description("Révoque immédiatement une clé, y compris la clé appelante si son identifiant est fourni.")]
    public Task<CallToolResult> RevokeKey(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new RevokeMcpApiKeyRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "archive_api_key", ReadOnly = false, Destructive = true)]
    [Description("Archive et révoque immédiatement une clé.")]
    public Task<CallToolResult> ArchiveKey(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new ArchiveMcpApiKeyRequest(id), cancellationToken), cancellationToken);

}
