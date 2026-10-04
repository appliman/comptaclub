namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record ValidateMcpApiKeyRequest(string PlainTextKey) : IRequest<McpApiKeyData?>;
