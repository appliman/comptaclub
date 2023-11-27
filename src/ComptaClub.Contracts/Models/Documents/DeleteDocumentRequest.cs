using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Documents;

public record DeleteDocumentRequest : IRequest<CommandResult>
{
    public DeleteDocumentRequest(Guid documentId)
    {
        DocumentId = documentId;
    }

    public Guid DocumentId { get; init; }
}
