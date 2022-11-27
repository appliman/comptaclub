namespace ComptaClub.Datas;

[Table("Documents")]
public class DocumentData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
}
