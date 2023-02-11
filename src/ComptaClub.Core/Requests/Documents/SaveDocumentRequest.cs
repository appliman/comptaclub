using DocumentFormat.OpenXml.Presentation;

namespace ComptaClub.Requests.Documents;

public class SaveDocumentRequest : IRequest<Results.PersistResult<Guid>>
{
    public SaveDocumentRequest(DocumentData document, MemoryStream? contentStream)
    {
        this.Entity = document;
        this.ContentStream = contentStream;
    }

    public SaveDocumentRequest(DocumentData document, string? fileName)
    {
        this.Entity = document;
        this.FileName = fileName;
    }

    public SaveDocumentRequest(DocumentData document, byte[]? data)
    {
        this.Entity = document;
        this.Content = data;
    }

    public SaveDocumentRequest(DocumentData document)
	{
		this.Entity = document;
	}

	public DocumentData Entity { get; init; }
    public MemoryStream? ContentStream { get; init; }
    public byte[]? Content { get; init; }
    public string? FileName { get; init; }

}
