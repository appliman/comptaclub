namespace ComptaClub.Datas;

[Table("RolesByUsers")]
public class RoleByUserData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
