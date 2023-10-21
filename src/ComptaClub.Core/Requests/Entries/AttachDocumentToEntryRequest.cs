using ComptaClub.Results;

namespace ComptaClub.Requests.Entries;

public record AttachDocumentToEntryRequest(Guid EntryId, Datas.DocumentData Document) 
	: IRequest<CommandResult>;
