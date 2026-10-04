namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record UpdateMcpApiKeyRequest(Guid Id, Guid ExpectedVersion, string Name, DateTime? ExpirationDateUtc = null) : IRequest<McpApiKeyResult>;
