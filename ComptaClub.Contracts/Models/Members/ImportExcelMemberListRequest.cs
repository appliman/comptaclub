namespace ComptaClub.Contracts.Models.Members;

public record ImportExcelMemberListRequest : IRequest<IEnumerable<MemberData>?>
{
    public ImportExcelMemberListRequest(MemoryStream? contentStream)
    {
        ContentStream = contentStream;
    }

    public ImportExcelMemberListRequest(string? fileName)
    {
        FileName = fileName;
    }

    public MemoryStream? ContentStream { get; init; }
    public string? FileName { get; init; }
}
