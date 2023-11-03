namespace ComptaClub.Datas;

[Table("ForecastBudgets")]
public class ForecastBudgetData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public Guid? IncomeStatementId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CreationDate { get; set; }
    public long CreditTotal { get; set; }
    public long DebitTotal { get; set; }
    public long IncomeStatementCreditTotal { get; set; }
    public long IncomeStatementDebitTotal { get; set; }
    [NotMapped]
    public List<ForecastBudgetItemData> ItemList { get; set; } = new();
}
