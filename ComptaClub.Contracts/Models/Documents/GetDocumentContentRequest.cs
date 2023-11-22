namespace ComptaClub.Contracts.Models.Documents;

public record GetDocumentContentRequest : IRequest<DocumentData?>
{
    public GetDocumentContentRequest(Guid documentId, Stream output)
    {
        DocumentId = documentId;
        Output = output;
    }

    public Guid DocumentId { get; init; }
    public Stream Output { get; init; }
}
