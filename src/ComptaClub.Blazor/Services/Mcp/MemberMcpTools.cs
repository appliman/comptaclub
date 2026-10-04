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
public sealed class MemberMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "list_members", ReadOnly = true, Destructive = false)]
    [Description("Recherche les membres avec pagination.")]
    public Task<CallToolResult> ListMembers(int pageIndex = 0, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default) =>
        Run(() => List<MemberData, MemberListFilter>(pageIndex, pageSize, search, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_member", ReadOnly = true, Destructive = false)]
    [Description("Retourne un membre.")]
    public Task<CallToolResult> GetMember(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<MemberData, MemberListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_member", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde un membre validé.")]
    public Task<CallToolResult> CreateMember(MemberInput input, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _member = await Mediator.Send(new CreateMemberRequest(), cancellationToken);
            Apply(_member, input);
            return await Mediator.Send(new SaveEntityRequest<MemberData>(_member), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_member", ReadOnly = false, Destructive = false)]
    [Description("Modifie un membre existant.")]
    public Task<CallToolResult> UpdateMember(Guid id, MemberInput input, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _member = await Find<MemberData, MemberListFilter>(id, cancellationToken);
            Apply(_member, input);
            return await Mediator.Send(new SaveEntityRequest<MemberData>(_member), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "delete_member", ReadOnly = false, Destructive = true)]
    [Description("Supprime un membre selon les règles existantes.")]
    public Task<CallToolResult> DeleteMember(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteMemberRequest(id), cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_member_balances", ReadOnly = true, Destructive = false)]
    [Description("Retourne les soldes des membres en millionièmes d’euro.")]
    public Task<CallToolResult> GetBalances(Guid? exerciceId = null, int pageIndex = 0, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new GetBalanceByMemberListRequest(new MemberListFilter
        {
            PageIndex = Math.Clamp(pageIndex, 0, 5000), PageSize = Math.Clamp(pageSize, 1, 100), Search = search
        }, exerciceId), cancellationToken), cancellationToken);

    [McpServerTool(Name = "import_members_excel", ReadOnly = false, Destructive = true)]
    [Description("Importe un fichier de membres XLSX de 512 Kio maximum, avec les validations métier.")]
    public Task<CallToolResult> ImportExcel(string fileName, string contentBase64, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            FileName(fileName);
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Un fichier XLSX est requis.");
            }
            using var _stream = new MemoryStream(DecodeFile(contentBase64));
            return await Mediator.Send(new ImportMembersExcelRequest(_stream), cancellationToken);
        }, cancellationToken);
    private static void Apply(MemberData member, MemberInput input)
    {
        if (!Enum.IsDefined(input.State))
        {
            throw new ArgumentException("État du membre invalide.");
        }
        member.Name = input.Name;
        member.Email = input.Email;
        member.State = input.State;
        member.LicenseNumber = input.LicenseNumber;
        member.LicenseTypeName = input.LicenseTypeName;
    }
}
