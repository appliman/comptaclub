namespace ComptaClub.Datas;

[Table("Entries")]
public class EntryData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = null!;
    public string Label { get; set; } = null!;
    public int CreationDate { get; set; }
    public long BalanceValue { get; set; }
    public long Amount { get; set; }
    public AccountDirection AccountDirection { get; set; }
    public Guid BankId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ExerciceId { get; set; }
    public Guid? UserCreatorId { get; set; }
    public Guid? MemberId { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? ExtraInfos { get; set; }
}
