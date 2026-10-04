namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record ListMcpApiKeysRequest(int PageIndex = 0, int PageSize = 50, string? Search = null, bool IncludeArchived = false) : IRequest<ComptaClub.Contracts.Models.PagedList<IEnumerable<McpApiKeyInfo>>>;
