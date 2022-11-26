namespace ComptaClub.Datas;

[Table("Members")]
public class Member : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
}
