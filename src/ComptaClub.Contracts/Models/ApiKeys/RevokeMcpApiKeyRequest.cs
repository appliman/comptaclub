namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record RevokeMcpApiKeyRequest(Guid Id) : IRequest<McpApiKeyResult>;
