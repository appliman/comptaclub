using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Entries;

public record AttachDocumentToEntryRequest(Guid EntryId, DocumentData Document)
    : IRequest<CommandResult>;
