namespace ComptaClub.Datas;

[Table("Roles")]
public class RoleData : IPrimaryKey
{
    [Key]
    public Guid Id{ get; set; }
    public string Name { get; set; } = null!;
}
