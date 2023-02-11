namespace ComptaClub.Requests.Documents;

public record GetDocumentContentRequest : IRequest<long>
{
	public GetDocumentContentRequest(Guid documentId, Stream output)
	{
		this.DocumentId = documentId;
		this.Output = output;
	}

	public Guid DocumentId { get; init; }
	public Stream Output { get; init; }
}
