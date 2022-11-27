namespace ComptaClub.Datas;

[Table("Accounts")]
public class AccountData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public AccountDirection Direction { get; set; }
    public string Label { get; set; } = null!;
    public int CreationDate { get; set; }
    public Guid? ParentAccountId { get; set; }
}
