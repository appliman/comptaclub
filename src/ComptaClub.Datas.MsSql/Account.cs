namespace ComptaClub.Datas;

[Table("Accounts")]
public class Account : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public int Direction { get; set; }
    public string Label { get; set; } = null!;
    public int CreationDate { get; set; }
    public Guid? ParentAccountId { get; set; }
}
