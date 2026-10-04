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
public sealed class ConfigurationMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "get_application_info", ReadOnly = true, Destructive = false)]
    [Description("Retourne les conventions de données et le contexte comptable actif.")]
    public Task<CallToolResult> GetInfo(CancellationToken cancellationToken = default) =>
        Run(async () => new
        {
            moneyScale = MONEY_SCALE, monetaryUnit = "millionième d’euro", dateEpoch = "2000-01-01",
            inputAmounts = "euros", inputDates = "ISO yyyy-MM-dd", maxFileBytes = MAX_FILE_BYTES,
            activeExercice = await Mediator.Send(new GetActiveExerciceRequest(), cancellationToken),
            activeBank = await Mediator.Send(new GetActiveBankRequest(), cancellationToken)
        }, cancellationToken);

    [McpServerTool(Name = "get_club", ReadOnly = true, Destructive = false)]
    [Description("Retourne la configuration du club.")]
    public Task<CallToolResult> GetClub(CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetClubRequest(), cancellationToken), cancellationToken);

    [McpServerTool(Name = "update_club", ReadOnly = false, Destructive = false)]
    [Description("Met à jour les champs de configuration du club.")]
    public Task<CallToolResult> UpdateClub(ClubInput input, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _club = await Mediator.Send(new GetClubRequest(), cancellationToken);
            _club.Name = input.Name;
            _club.Object = input.Object;
            _club.Address = input.Address;
            _club.SirenNumber = input.SirenNumber;
            _club.SiretNumber = input.SiretNumber;
            _club.RNA = input.Rna;
            _club.Email = input.Email;
            _club.WebSite = input.WebSite;
            _club.PhoneNumber = input.PhoneNumber;
            _club.ContactName = input.ContactName;
            if (input.LogoBase64 is not null)
            {
                DecodeFile(input.LogoBase64);
                _club.LogoBase64String = input.LogoBase64;
                _club.LogoContentType = input.LogoContentType;
            }
            return await Mediator.Send(new SaveClubRequest(_club), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "list_users", ReadOnly = true, Destructive = false)]
    [Description("Recherche les utilisateurs.")]
    public Task<CallToolResult> ListUsers(int pageIndex = 0, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default) =>
        Run(() => List<UserData, UserListFilter>(pageIndex, pageSize, search, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_user", ReadOnly = true, Destructive = false)]
    [Description("Retourne un utilisateur.")]
    public Task<CallToolResult> GetUser(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<UserData, UserListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_user", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde un utilisateur.")]
    public Task<CallToolResult> CreateUser(string name, string email, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _user = await Mediator.Send(new CreateUserRequest(name, email), cancellationToken);
            return await Mediator.Send(new SaveEntityRequest<UserData>(_user), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_user", ReadOnly = false, Destructive = false)]
    [Description("Modifie le nom et l’adresse d’un utilisateur existant.")]
    public Task<CallToolResult> UpdateUser(Guid id, string name, string email, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _user = await Find<UserData, UserListFilter>(id, cancellationToken);
            _user.Name = name;
            _user.Email = email;
            return await Mediator.Send(new SaveEntityRequest<UserData>(_user), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "disable_user", ReadOnly = false, Destructive = true)]
    [Description("Désactive un utilisateur et invalide ses clés MCP.")]
    public Task<CallToolResult> DisableUser(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DisableUserRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "enable_user", ReadOnly = false, Destructive = false)]
    [Description("Réactive un utilisateur désactivé.")]
    public Task<CallToolResult> EnableUser(Guid id, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _user = await Find<UserData, UserListFilter>(id, cancellationToken);
            _user.DisableDate = null;
            return await Mediator.Send(new SaveEntityRequest<UserData>(_user), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "list_banks", ReadOnly = true, Destructive = false)]
    [Description("Liste les banques avec pagination.")]
    public Task<CallToolResult> ListBanks(int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _items = (await Mediator.Send(new GetAllBanksRequest(), cancellationToken)).OrderBy(item => item.Code).ToList();
            var _size = Math.Clamp(pageSize, 1, 100);
            return new { items = _items.Skip(Math.Clamp(pageIndex, 0, 5000) * _size).Take(_size), totalCount = _items.Count };
        }, cancellationToken);

    [McpServerTool(Name = "get_bank", ReadOnly = true, Destructive = false)]
    [Description("Retourne une banque.")]
    public Task<CallToolResult> GetBank(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetBankByFilterRequest(item => item.Id == id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_bank", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde une banque.")]
    public Task<CallToolResult> CreateBank(string code, string label, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _bank = await Mediator.Send(new CreateBankRequest(code, label), cancellationToken);
            return await Mediator.Send(new SaveEntityRequest<BankData>(_bank), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_bank", ReadOnly = false, Destructive = false)]
    [Description("Modifie une banque existante sans changer son activation.")]
    public Task<CallToolResult> UpdateBank(Guid id, string code, string label, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _bank = await Mediator.Send(new GetBankByFilterRequest(item => item.Id == id), cancellationToken)
                ?? throw new KeyNotFoundException("Banque introuvable.");
            _bank.Code = code;
            _bank.Label = label;
            return await Mediator.Send(new SaveEntityRequest<BankData>(_bank), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "activate_bank", ReadOnly = false, Destructive = false)]
    [Description("Change la banque active selon les règles existantes.")]
    public Task<CallToolResult> ActivateBank(Guid id, bool active = true, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new ChangeActiveBankRequest(active, id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "delete_bank", ReadOnly = false, Destructive = true)]
    [Description("Supprime une banque selon les règles existantes.")]
    public Task<CallToolResult> DeleteBank(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteBankRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "list_exercices", ReadOnly = true, Destructive = false)]
    [Description("Liste les exercices avec pagination.")]
    public Task<CallToolResult> ListExercices(int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _items = (await Mediator.Send(new GetAllExercicesRequest(), cancellationToken)).OrderBy(item => item.Code).ToList();
            var _size = Math.Clamp(pageSize, 1, 100);
            return new { items = _items.Skip(Math.Clamp(pageIndex, 0, 5000) * _size).Take(_size), totalCount = _items.Count };
        }, cancellationToken);

    [McpServerTool(Name = "get_exercice", ReadOnly = true, Destructive = false)]
    [Description("Retourne un exercice.")]
    public Task<CallToolResult> GetExercice(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetExerciceByFilterRequest(item => item.Id == id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_exercice", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde un exercice ; solde initial en euros.")]
    public Task<CallToolResult> CreateExercice(string code, string label, DateOnly startDate, DateOnly endDate, decimal initialAmountEuros = 0, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _exercice = await Mediator.Send(new CreateExerciceRequest(code, label, SignedMoney(initialAmountEuros)), cancellationToken);
            _exercice.StartDate = startDate.ToDateTime(TimeOnly.MinValue).ToDayId();
            _exercice.EndDate = endDate.ToDateTime(TimeOnly.MinValue).ToDayId();
            return await Mediator.Send(new SaveEntityRequest<ExerciceData>(_exercice), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_exercice", ReadOnly = false, Destructive = false)]
    [Description("Modifie un exercice ouvert, sans modifier ses soldes calculés.")]
    public Task<CallToolResult> UpdateExercice(Guid id, string code, string label, DateOnly startDate, DateOnly endDate, decimal initialAmountEuros, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _exercice = await Mediator.Send(new GetExerciceByFilterRequest(item => item.Id == id), cancellationToken)
                ?? throw new KeyNotFoundException("Exercice introuvable.");
            if (_exercice.ClosedDate != null || _exercice.ExerciceState == ExerciceState.Closed)
            {
                throw new ArgumentException("Un exercice clos ne peut pas être modifié.");
            }
            _exercice.Code = code;
            _exercice.Label = label;
            _exercice.StartDate = startDate.ToDateTime(TimeOnly.MinValue).ToDayId();
            _exercice.EndDate = endDate.ToDateTime(TimeOnly.MinValue).ToDayId();
            _exercice.InitialAmount = SignedMoney(initialAmountEuros);
            return await Mediator.Send(new SaveEntityRequest<ExerciceData>(_exercice), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "create_next_exercice", ReadOnly = false, Destructive = false)]
    [Description("Prépare et sauvegarde l’exercice suivant selon les règles de report.")]
    public Task<CallToolResult> CreateNextExercice(Guid sourceId, string code, string label, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _exercice = await Mediator.Send(new CreateNextExerciceRequest(sourceId, code, label), cancellationToken)
                ?? throw new KeyNotFoundException("Exercice source introuvable.");
            return await Mediator.Send(new SaveEntityRequest<ExerciceData>(_exercice), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "activate_exercice", ReadOnly = false, Destructive = false)]
    [Description("Change l’exercice actif selon les règles existantes.")]
    public Task<CallToolResult> ActivateExercice(Guid id, bool active = true, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new ChangeActiveExerciceRequest(active, id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "close_exercice", ReadOnly = false, Destructive = true)]
    [Description("Clôture un exercice ; cette opération empêche la modification de ses écritures.")]
    public Task<CallToolResult> CloseExercice(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new CloseExerciceRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "delete_exercice", ReadOnly = false, Destructive = true)]
    [Description("Supprime un exercice selon les règles existantes.")]
    public Task<CallToolResult> DeleteExercice(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteExerciceRequest(id), cancellationToken), cancellationToken);
    private static long SignedMoney(decimal euros) => euros < 0 ? -Money(-euros) : Money(euros);
}
