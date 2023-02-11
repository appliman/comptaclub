using ComptaClub.Results;

namespace ComptaClub.Requests.Documents;

public record DeleteDocumentRequest : IRequest<CommandResult>
{
    public DeleteDocumentRequest(Guid documentId)
    {
        DocumentId = documentId;
    }

    public Guid DocumentId { get; init; }
}
