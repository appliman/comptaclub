using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Entries;

public sealed record SaveOfxEntryRequest(EntryData Entry) : IRequest<PersistResult>;
