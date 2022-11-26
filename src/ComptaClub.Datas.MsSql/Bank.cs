namespace ComptaClub.Datas;

[Table("Banks")]
public class Bank : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Label { get; set; }
    public int CreationDate { get; set; }

}
