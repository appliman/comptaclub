using System.ComponentModel.DataAnnotations.Schema;

namespace ComptaClub.Blazor.ViewModels;

public class IncomeStatementItem
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
	public List<IncomeStatementItem> Children { get; set; } = new();

}
