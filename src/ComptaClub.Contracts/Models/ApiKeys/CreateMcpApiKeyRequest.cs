namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record CreateMcpApiKeyRequest(string Name, DateTime? ExpirationDateUtc = null) : IRequest<McpApiKeyResult>;
