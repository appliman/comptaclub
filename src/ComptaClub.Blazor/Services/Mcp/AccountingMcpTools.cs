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
public sealed class AccountingMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "get_current_balance", ReadOnly = true, Destructive = false)]
    [Description("Retourne le solde courant en millionièmes d’euro.")]
    public Task<CallToolResult> GetBalance(CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetCurrentBalanceRequest(), cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_daily_balances", ReadOnly = true, Destructive = false)]
    [Description("Retourne les soldes journaliers en euros avec pagination.")]
    public Task<CallToolResult> GetDailyBalances(Guid? exerciceId = null, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _items = (await Mediator.Send(new GetBalanceByDayRequest(exerciceId), cancellationToken)).ToList();
            var _size = Math.Clamp(pageSize, 1, 100);
            return new { items = _items.Skip(Math.Clamp(pageIndex, 0, 5000) * _size).Take(_size), totalCount = _items.Count };
        }, cancellationToken);

    [McpServerTool(Name = "list_income_statements", ReadOnly = true, Destructive = false)]
    [Description("Liste les comptes de résultats.")]
    public Task<CallToolResult> ListIncomeStatements(int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(() => List<IncomeStatementData, IncomeStatementListFilter>(pageIndex, pageSize, null, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_income_statement", ReadOnly = true, Destructive = false)]
    [Description("Retourne les totaux d’un compte de résultats.")]
    public Task<CallToolResult> GetIncomeStatement(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<IncomeStatementData, IncomeStatementListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "list_income_statement_items", ReadOnly = true, Destructive = false)]
    [Description("Retourne les lignes du rapport de compte de résultats.")]
    public Task<CallToolResult> ListIncomeStatementItems(Guid id, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(() => List<IncomeStatementItemData, IncomeStatementItemListFilter>(pageIndex, pageSize, null, cancellationToken, filter => filter.IncomeStatementId = id), cancellationToken);

    [McpServerTool(Name = "create_income_statement", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde le compte de résultats d’un exercice.")]
    public Task<CallToolResult> CreateIncomeStatement(Guid exerciceId, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new CreateAndSaveIncomeStatementRequest(exerciceId), cancellationToken), cancellationToken);

    [McpServerTool(Name = "delete_income_statement", ReadOnly = false, Destructive = true)]
    [Description("Supprime un compte de résultats.")]
    public Task<CallToolResult> DeleteIncomeStatement(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteIncomeStatementRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "list_forecast_budgets", ReadOnly = true, Destructive = false)]
    [Description("Liste les budgets prévisionnels.")]
    public Task<CallToolResult> ListBudgets(int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(() => List<ForecastBudgetData, ForecastBudgetListFilter>(pageIndex, pageSize, null, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_forecast_budget", ReadOnly = true, Destructive = false)]
    [Description("Retourne les totaux d’un budget prévisionnel.")]
    public Task<CallToolResult> GetBudget(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<ForecastBudgetData, ForecastBudgetListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "list_forecast_budget_items", ReadOnly = true, Destructive = false)]
    [Description("Retourne les lignes du rapport prévisionnel.")]
    public Task<CallToolResult> ListBudgetItems(Guid id, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(() => List<ForecastBudgetItemData, ForecastBudgetItemListFilter>(pageIndex, pageSize, null, cancellationToken, filter => filter.ForeCastBudgetId = id), cancellationToken);

    [McpServerTool(Name = "create_forecast_budget", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde un budget, éventuellement à partir d’un compte de résultats.")]
    public Task<CallToolResult> CreateBudget(string name, string description, Guid? incomeStatementId = null, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Un nom de budget est requis.");
            }
            if (incomeStatementId.HasValue)
            {
                await Find<IncomeStatementData, IncomeStatementListFilter>(incomeStatementId.Value, cancellationToken);
                return await Mediator.Send(new CreateAndSaveForecastBudgetFromIncomeStatementRequest(incomeStatementId.Value, name, description), cancellationToken);
            }
            return await Mediator.Send(new CreateAndSaveForecastBudgetRequest(name, description), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_forecast_budget", ReadOnly = false, Destructive = false)]
    [Description("Modifie un budget et ses montants en euros. Les identifiants de lignes doivent appartenir au budget.")]
    public Task<CallToolResult> UpdateBudget(Guid id, string name, string description, ForecastAmountInput[] amounts, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(name) || amounts.Select(item => item.ItemId).Distinct().Count() != amounts.Length)
            {
                throw new ArgumentException("Nom requis et identifiants de lignes uniques.");
            }
            var _budget = await Find<ForecastBudgetData, ForecastBudgetListFilter>(id, cancellationToken);
            var _items = new List<ForecastBudgetItemData>();
            for (var _page = 0; ; _page++)
            {
                var _batch = await List<ForecastBudgetItemData, ForecastBudgetItemListFilter>(_page, 100, null, cancellationToken, filter => filter.ForeCastBudgetId = id);
                _items.AddRange(_batch.List);
                if (_batch.List.Count() < 100)
                {
                    break;
                }
            }
            foreach (var _amount in amounts)
            {
                var _item = _items.SingleOrDefault(item => item.Id == _amount.ItemId)
                    ?? throw new ArgumentException("Une ligne n’appartient pas à ce budget.");
                if (_items.Any(item => item.ParentForecastBudgetItemId == _item.Id))
                {
                    throw new ArgumentException("Modifiez les lignes feuilles ; les totaux sont calculés.");
                }
                _item.Amount = Money(_amount.AmountEuros);
            }
            _budget.Name = name;
            _budget.Description = description;
            _budget.ItemList = _items;
            _budget.ItemList.Levelize();
            _budget.ItemList.Hierarchize();
            _budget.ComputeTotal();
            return await Mediator.Send(new SaveForecastBudgetRequest(_budget), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "delete_forecast_budget", ReadOnly = false, Destructive = true)]
    [Description("Supprime un budget prévisionnel.")]
    public Task<CallToolResult> DeleteBudget(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteForecastBudgetRequest(id), cancellationToken), cancellationToken);

}
