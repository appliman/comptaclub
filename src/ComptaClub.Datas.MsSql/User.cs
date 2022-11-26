namespace ComptaClub.Datas;

[Table("Users")]
public class User : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
}
