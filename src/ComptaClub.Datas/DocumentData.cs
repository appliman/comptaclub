namespace ComptaClub.Datas;

[Table("Documents")]
public class DocumentData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public int CreationDate { get; set; }
    public int LastUpdate { get; set; }
    public string FileName { get; set; } = null!;
    public string? Description { get; set; }
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
    public Guid? UserOwnerId { get; set; }
}
