using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Members;

public sealed class ImportMembersExcelResult : CommandResult
{
    public List<Guid> ImportedIds { get; } = [];
    public int ImportedCount => ImportedIds.Count;
}
