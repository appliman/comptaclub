using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels;

public class IncomeStatement
{
    public Guid Id { get; set; }
    public Guid ExerciceId { get; set; }
    public int CreationDate { get; set; }
    public long CreditTotal { get; set; }
    public long DebitTotal { get; set; }
    public string Description { get; set; } = null!;
    public List<IncomeStatementItem> ItemList { get; set; } = new();
}
