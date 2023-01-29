namespace ComptaClub.Requests;

public class ImportExcelMemberListRequest : IRequest<IEnumerable<Datas.MemberData>?>
{
    public ImportExcelMemberListRequest(MemoryStream? contentStream)
    {
        this.ContentStream = contentStream;
    }

    public ImportExcelMemberListRequest(string? fileName)
    {
        this.FileName = fileName;
    }

    public MemoryStream? ContentStream { get; init; }
    public string? FileName { get; init; }
}
