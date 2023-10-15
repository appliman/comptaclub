namespace ComptaClub.Datas;

[Table("IncomeStatementItems")]
public class IncomeStatementItemData : IPrimaryKey
{
    public Guid Id { get; set; }
    public Guid IncomeStatementId { get; set; }
    public Guid? ParentIncomeStatementItemId { get; set; }
    public Guid AccountId { get; set; }
    public long Amount { get; set; }
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
    public AccountDirection Direction { get; set; }
	[NotMapped]
	public int Level { get; set; } = -1;

}
