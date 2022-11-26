namespace ComptaClub.Datas;

[Table("Documents")]
public class Document : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
}
