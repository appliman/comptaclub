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
public sealed class DocumentMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "list_documents", ReadOnly = true, Destructive = false)]
    [Description("Recherche les métadonnées des documents, sans contenu binaire.")]
    public Task<CallToolResult> ListDocuments(int pageIndex = 0, int pageSize = 50, string? search = null, CancellationToken cancellationToken = default) =>
        Run(() => List<DocumentData, DocumentListFilter>(pageIndex, pageSize, search, cancellationToken), cancellationToken);

    [McpServerTool(Name = "get_document", ReadOnly = true, Destructive = false)]
    [Description("Retourne les métadonnées d’un document.")]
    public Task<CallToolResult> GetDocument(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Find<DocumentData, DocumentListFilter>(id, cancellationToken), cancellationToken);

    [McpServerTool(Name = "upload_document", ReadOnly = false, Destructive = false)]
    [Description("Crée un document de 512 Kio maximum avec son contenu base64.")]
    public Task<CallToolResult> UploadDocument(string fileName, string mimeType, string contentBase64, string? description = null, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _bytes = DecodeFile(contentBase64);
            var _document = await Mediator.Send(new CreateDocumentRequest(), cancellationToken);
            _document.FileName = FileName(fileName);
            _document.MimeType = mimeType;
            _document.Description = description;
            _document.Size = _bytes.Length;
            _document.UserOwnerId = await Actor(cancellationToken);
            return await Mediator.Send(new SaveDocumentRequest(_document, _bytes), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "update_document", ReadOnly = false, Destructive = false)]
    [Description("Modifie un document ; sans contenuBase64, conserve son contenu.")]
    public Task<CallToolResult> UpdateDocument(Guid id, string fileName, string mimeType, string? description = null, string? contentBase64 = null, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _document = await Find<DocumentData, DocumentListFilter>(id, cancellationToken);
            _document.FileName = FileName(fileName);
            _document.MimeType = mimeType;
            _document.Description = description;
            return contentBase64 is null
                ? await Mediator.Send(new SaveDocumentRequest(_document), cancellationToken)
                : await Mediator.Send(new SaveDocumentRequest(_document, DecodeFile(contentBase64)), cancellationToken);
        }, cancellationToken);

    [McpServerTool(Name = "get_document_content", ReadOnly = true, Destructive = false)]
    [Description("Télécharge un document par blocs base64, jusqu’à 256 Kio. Retourne l’offset suivant et la taille totale.")]
    public Task<CallToolResult> GetContent(Guid id, int offset = 0, int count = 262144, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (offset < 0 || count < 1 || count > 262144)
            {
                throw new ArgumentException("Offset positif et bloc de 1 à 262144 octets requis.");
            }
            using var _stream = new MemoryStream();
            var _document = await Mediator.Send(new GetDocumentContentRequest(id, _stream), cancellationToken)
                ?? throw new KeyNotFoundException("Document introuvable.");
            if (offset > _stream.Length)
            {
                throw new ArgumentException("Offset au-delà du document.");
            }
            var _length = (int)Math.Min(count, _stream.Length - offset);
            return new { _document.FileName, _document.MimeType, totalBytes = _stream.Length,
                contentBase64 = Convert.ToBase64String(_stream.GetBuffer(), offset, _length),
                nextOffset = offset + _length, completed = offset + _length == _stream.Length };
        }, cancellationToken);

    [McpServerTool(Name = "delete_document", ReadOnly = false, Destructive = true)]
    [Description("Supprime un document et ses associations selon les règles existantes.")]
    public Task<CallToolResult> DeleteDocument(Guid id, CancellationToken cancellationToken = default) =>
        Run(() => Mediator.Send(new DeleteDocumentRequest(id), cancellationToken), cancellationToken);

}
