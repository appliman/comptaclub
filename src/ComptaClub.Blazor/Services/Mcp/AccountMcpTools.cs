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
public sealed class AccountMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "list_accounts", ReadOnly = true, Destructive = false)]
    [Description("Recherche les comptes du plan comptable avec pagination.")]
    public Task<CallToolResult> ListAccounts(int pageIndex = 0, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default) =>
        Run(() => List<AccountData, AccountListFilter>(pageIndex, pageSize, search, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_account", ReadOnly = true, Destructive = false)]
    [Description("Retourne un compte.")]
    public Task<CallToolResult> GetAccount(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<AccountData, AccountListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_account", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde un compte validé.")]
    public Task<CallToolResult> CreateAccount(string code, string label, AccountDirection direction, Guid? parentId = null, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (!Enum.IsDefined(direction))
            {
                throw new ArgumentException("Sens du compte invalide.");
            }
            var _account = await Mediator.Send(new CreateAccountRequest(code, label, direction) { ParentId = parentId }, cancellationToken);
            return await Mediator.Send(new SaveEntityRequest<AccountData>(_account), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_account", ReadOnly = false, Destructive = false)]
    [Description("Modifie les champs saisissables d’un compte existant.")]
    public Task<CallToolResult> UpdateAccount(Guid id, string code, string label, AccountDirection direction, Guid? parentId = null, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (!Enum.IsDefined(direction))
            {
                throw new ArgumentException("Sens du compte invalide.");
            }
            var _account = await Find<AccountData, AccountListFilter>(id, cancellationToken);
            _account.Code = code;
            _account.Label = label;
            _account.Direction = direction;
            _account.ParentAccountId = parentId;
            return await Mediator.Send(new SaveEntityRequest<AccountData>(_account), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "delete_account", ReadOnly = false, Destructive = true)]
    [Description("Supprime un compte si les règles comptables le permettent.")]
    public Task<CallToolResult> DeleteAccount(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteAccountRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "export_accounting_plan", ReadOnly = true, Destructive = false)]
    [Description("Exporte le plan comptable hiérarchique JSON en base64, sans fichier serveur.")]
    public Task<CallToolResult> ExportPlan(CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _plan = await Mediator.Send(new GetPlanRequest(), cancellationToken);
            var _bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_plan, ComptaClub.JsonSerializer.Options);
            return new { fileName = "plan-comptable.json", mimeType = "application/json", contentBase64 = Convert.ToBase64String(_bytes) };
        }, cancellationToken);

    [McpServerTool(Name = "import_accounting_plan", ReadOnly = false, Destructive = true)]
    [Description("Importe un plan comptable JSON de 512 Kio maximum avec les règles métier.")]
    public Task<CallToolResult> ImportPlan(string contentBase64, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _plan = System.Text.Json.JsonSerializer.Deserialize<List<AccountData>>(DecodeFile(contentBase64), ComptaClub.JsonSerializer.Options)
                ?? throw new ArgumentException("Plan JSON invalide.");
            return await Mediator.Send(new ImportAccountingPlanRequest(_plan), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "get_account_totals", ReadOnly = true, Destructive = false)]
    [Description("Retourne les totaux par compte, en millionièmes d’euro.")]
    public Task<CallToolResult> GetTotals(Guid? exerciceId = null, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _items = (await Mediator.Send(new GetAmountTotalByAccountRequest(exerciceId), cancellationToken)).ToList();
            var _size = Math.Clamp(pageSize, 1, 100);
            return new { items = _items.Skip(Math.Clamp(pageIndex, 0, 5000) * _size).Take(_size), totalCount = _items.Count };
        }, cancellationToken);

}
