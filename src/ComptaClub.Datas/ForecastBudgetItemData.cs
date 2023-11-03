namespace ComptaClub.Datas;

[Table("ForecastBudgetItems")]
public class ForecastBudgetItemData : IPrimaryKey, ICloneable
{
    [Key]
    public Guid Id { get; set; }
    public Guid ForecastBudgetId { get; set; }
    public Guid? ParentForecastBudgetItemId { get; set; }
    public Guid AccountId { get; set; }
    public long? IncomeStatementAmount { get; set; }
    public long Amount { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountLabel { get; set; } = null!;
    public AccountDirection Direction { get; set; }
    [NotMapped]
    public int Level { get; set; } = -1;
    [NotMapped]
    public List<ForecastBudgetItemData> Children { get; set; } = new();

    public object Clone()
    {
        var clone = (ForecastBudgetItemData)MemberwiseClone();
        clone.Children = new();
        return clone;
    }
}
