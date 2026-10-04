using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Clubs;

public sealed class ZippedDatabaseResult : CommandResult
{
    public string? RelativeUrl { get; set; }
}
