using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Entries;
public record RemoveDocumentFromEntryRequest(Guid EntryId, Guid DocumentId)
    : IRequest<CommandResult>;