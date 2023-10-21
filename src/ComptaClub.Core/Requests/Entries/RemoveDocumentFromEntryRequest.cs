using ComptaClub.Results;

namespace ComptaClub.Requests.Entries;
public record RemoveDocumentFromEntryRequest(Guid EntryId, Guid DocumentId)
	: IRequest<CommandResult>;