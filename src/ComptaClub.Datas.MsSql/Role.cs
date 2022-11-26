namespace ComptaClub.Datas;

[Table("Roles")]
public class Role : IPrimaryKey
{
    [Key]
    public Guid Id{ get; set; }
}
