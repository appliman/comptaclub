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
public sealed class EntryMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "list_entries", ReadOnly = true, Destructive = false)]
    [Description("Recherche les écritures. Montants stockés en millionièmes d’euro ; dates en jours depuis le 01/01/2000.")]
    public Task<CallToolResult> ListEntries(int pageIndex = 0, int pageSize = 50, string? search = null, Guid? exerciceId = null, Guid? accountId = null, CancellationToken cancellationToken = default) =>
        Run(() => List<EntryData, EntryListFilter>(pageIndex, pageSize, search, cancellationToken, filter =>
        {
            filter.ExerciceId = exerciceId;
            filter.AccountIdList = accountId.HasValue ? [accountId.Value] : null;
        }), cancellationToken);

    [McpServerTool(Name = "get_entry", ReadOnly = true, Destructive = false)]
    [Description("Retourne une écriture par identifiant.")]
    public Task<CallToolResult> GetEntry(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<EntryData, EntryListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "create_entry", ReadOnly = false, Destructive = false)]
    [Description("Crée et sauvegarde une écriture validée. Montant en euros, dates ISO.")]
    public Task<CallToolResult> CreateEntry(EntryInput input, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _entry = await Mediator.Send(new CreateEntryRequest(), cancellationToken);
            Apply(_entry, input);
            _entry.UserCreatorId = await Actor(cancellationToken);
            return await Mediator.Send(new SaveEntityRequest<EntryData>(_entry), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_entry", ReadOnly = false, Destructive = false)]
    [Description("Modifie les champs saisissables d’une écriture existante en conservant son créateur.")]
    public Task<CallToolResult> UpdateEntry(Guid id, EntryInput input, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(id, cancellationToken);
            var _entry = await Find<EntryData, EntryListFilter>(id, cancellationToken);
            Apply(_entry, input);
            return await Mediator.Send(new SaveEntityRequest<EntryData>(_entry), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "delete_entry", ReadOnly = false, Destructive = true)]
    [Description("Supprime une écriture selon les règles comptables ; les exercices clos sont protégés.")]
    public Task<CallToolResult> DeleteEntry(Guid id, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(id, cancellationToken);
            return await Mediator.Send(new DeleteEntryRequest(id), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "list_entry_members", ReadOnly = true, Destructive = false)]
    [Description("Liste les associations entre une écriture et les membres.")]
    public Task<CallToolResult> ListEntryMembers(Guid entryId, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _items = (await Mediator.Send(new GetAssociatedMemberListByEntryRequest(entryId), cancellationToken)).OrderBy(item => item.Id).ToList();
            var _size = Math.Clamp(pageSize, 1, 100);
            return new { items = _items.Skip(Math.Clamp(pageIndex, 0, 5000) * _size).Take(_size), totalCount = _items.Count };
        }, cancellationToken);

    [McpServerTool(Name = "link_member_to_entry", ReadOnly = false, Destructive = false)]
    [Description("Associe un membre à une écriture ; montant en euros.")]
    public Task<CallToolResult> LinkMember(Guid entryId, Guid memberId, decimal amountEuros, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(entryId, cancellationToken);
            await Find<MemberData, MemberListFilter>(memberId, cancellationToken);
            return await Mediator.Send(new LinkMemberToEntryRequest(entryId, memberId, Money(amountEuros)), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "unlink_member_from_entry", ReadOnly = false, Destructive = true)]
    [Description("Retire une association membre-écriture par son identifiant.")]
    public Task<CallToolResult> UnlinkMember(Guid entryId, Guid associationId, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(entryId, cancellationToken);
            var _links = await Mediator.Send(new GetAssociatedMemberListByEntryRequest(entryId), cancellationToken);
            if (!_links.Any(item => item.Id == associationId))
            {
                throw new KeyNotFoundException("Association introuvable pour cette écriture.");
            }
            return await Mediator.Send(new UnlinkMemberToEntryRequest(associationId), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "list_entry_documents", ReadOnly = true, Destructive = false)]
    [Description("Liste les documents associés à une écriture.")]
    public Task<CallToolResult> ListDocuments(Guid entryId, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default) =>
        Run(() => List<DocumentData, DocumentListFilter>(pageIndex, pageSize, null, cancellationToken,
            filter => filter.MetaEntityIdList = new MetaEntityIdList(MetaEntity.Entry, new List<Guid> { entryId })), cancellationToken);

    [McpServerTool(Name = "attach_document_to_entry", ReadOnly = false, Destructive = false)]
    [Description("Associe un document existant à une écriture.")]
    public Task<CallToolResult> AttachDocument(Guid entryId, Guid documentId, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(entryId, cancellationToken);
            var _document = await Find<DocumentData, DocumentListFilter>(documentId, cancellationToken);
            return await Mediator.Send(new AttachDocumentToEntryRequest(entryId, _document), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "detach_document_from_entry", ReadOnly = false, Destructive = true)]
    [Description("Retire un document d’une écriture sans supprimer le document.")]
    public Task<CallToolResult> DetachDocument(Guid entryId, Guid documentId, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            await EnsureWritableEntry(entryId, cancellationToken);
            return await Mediator.Send(new RemoveDocumentFromEntryRequest(entryId, documentId), cancellationToken);
        }, cancellationToken);
    internal static void Apply(EntryData entry, EntryInput input)
    {
        if (!Enum.IsDefined(input.Direction) || !Enum.IsDefined(input.PaymentType))
        {
            throw new ArgumentException("Sens ou mode de paiement invalide.");
        }
        entry.PartNumber = input.PartNumber;
        entry.Label = input.Label;
        entry.CreationDate = input.CreationDate.ToDateTime(TimeOnly.MinValue).ToDayId();
        entry.ValueDate = input.ValueDate.ToDateTime(TimeOnly.MinValue).ToDayId();
        entry.Amount = Money(input.AmountEuros);
        entry.AccountDirection = input.Direction;
        entry.BankId = input.BankId;
        entry.AccountId = input.AccountId;
        entry.ExerciceId = input.ExerciceId;
        entry.PaymentType = input.PaymentType;
        entry.ExtraInfos = input.ExtraInfos;
    }
}
