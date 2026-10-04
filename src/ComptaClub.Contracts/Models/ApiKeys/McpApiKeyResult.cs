using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed class McpApiKeyResult : PersistResult
{
    public McpApiKeyInfo? ApiKey { get; set; }
    public string? PlainTextKey { get; set; }
}
