namespace ComptaClub.Datas;

[Table("IncomeStatements")]
public class IncomeStatementData : IPrimaryKey
{
    public Guid Id { get; set; }
    public Guid ExerciceId { get; set; }
    public int CreationDate { get; set; }
    public long CreditTotal { get; set; }
    public long DebitTotal { get; set; }
    public string Description { get; set; } = null!;
}
