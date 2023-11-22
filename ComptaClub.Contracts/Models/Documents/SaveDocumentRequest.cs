using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Documents;

public class SaveDocumentRequest : IRequest<PersistResult>
{
	public SaveDocumentRequest(DocumentData document, MemoryStream? contentStream)
	{
		Entity = document;
		ContentStream = contentStream;
	}

	public SaveDocumentRequest(DocumentData document, string? fileName)
	{
		Entity = document;
		FileName = fileName;
	}

	public SaveDocumentRequest(DocumentData document, byte[]? data)
	{
		Entity = document;
		Content = data;
	}

	public SaveDocumentRequest(DocumentData document)
	{
		Entity = document;
	}

	public DocumentData Entity { get; init; }
	public MemoryStream? ContentStream { get; init; }
	public byte[]? Content { get; init; }
	public string? FileName { get; init; }

}
