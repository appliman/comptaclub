namespace ComptaClub.Datas;

[Table("Banks")]
public class BankData : IPrimaryKey, IActivable
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Label { get; set; }
    public int CreationDate { get; set; }
    public bool Active { get; set; }

}
