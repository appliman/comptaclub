namespace ComptaClub.Datas;

[Table("DocumentContents")]
public class DocumentContentData
{
    [Key]
    public Guid DocumentId { get; set; }
    public byte[] Content { get; set; } = null!;

}
