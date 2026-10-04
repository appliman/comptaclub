namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record ArchiveMcpApiKeyRequest(Guid Id) : IRequest<McpApiKeyResult>;
