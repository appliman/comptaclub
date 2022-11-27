namespace ComptaClub.Datas;

[Table("Members")]
public class MemberData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
}
