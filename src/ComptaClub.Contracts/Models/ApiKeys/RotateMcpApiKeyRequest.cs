namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record RotateMcpApiKeyRequest(Guid Id, Guid ExpectedVersion) : IRequest<McpApiKeyResult>;
