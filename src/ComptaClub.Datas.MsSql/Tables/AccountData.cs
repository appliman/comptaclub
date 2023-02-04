namespace ComptaClub.Datas;

[Table("Accounts")]
public class AccountData : IPrimaryKey, ICloneable
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public AccountDirection Direction { get; set; }
    public string Label { get; set; } = null!;
    public int CreationDate { get; set; }
    public Guid? ParentAccountId { get; set; }
    [NotMapped]
    public List<AccountData> Children { get; set; } = new();
    [NotMapped]
    public int Level { get; set; } = -1;

    public object Clone()
    {
        var clone = this.MemberwiseClone();
        return clone;
    }
}
