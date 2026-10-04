namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record GetMcpApiKeyRequest(Guid Id) : IRequest<McpApiKeyInfo?>;
