using ComptaClub.Contracts.Results;

namespace ComptaClub.Blazor.Services.Mcp;

public sealed class OfxBatchResult : CommandResult
{
    public List<PersistResult> Items { get; set; } = [];
}
