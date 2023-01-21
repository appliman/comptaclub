namespace ComptaClub.Datas;

[Table("Users")]
public class UserData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int CreationDate { get; set; }
    public int? DisableDate { get; set; }
}
