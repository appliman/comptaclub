using System.ComponentModel;

namespace ComptaClub.Requests.ForecastBudget;
public record ForecastBudgetItemListFilter : IListFilter
{
    public List<Guid> IdList { get; set; } = new();
    public int PageIndex { get; set; }
    public int? Skip { get; set; }
    public int PageSize { get; set; } = int.MaxValue;
    public string? SortByName { get; set; }
    public ListSortDirection SortDirection { get; set; }
    public string? Search { get; set; }
    public ComputeRowCount ComputeRowCount { get; set; } = ComputeRowCount.OnlyInFirstPage;
    public string? Code { get; set; }
    public Guid? ForeCastBudgetId { get; set; }

    public void GetById(Guid entryId)
    {
        IdList.Clear();
        IdList.Add(entryId);
    }
}