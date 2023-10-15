using System.ComponentModel;

namespace ComptaClub.Models;

public class MemberListFilter : IListFilter
{
    public List<Guid> IdList { get; set; } = new();
    public int PageIndex { get; set; }
    public int? Skip { get; set; }
    public int PageSize { get; set; } = int.MaxValue;
    public string? SortByName { get; set; }
    public ListSortDirection SortDirection { get; set; }
    public string? Search { get; set; }
    public ComputeRowCount ComputeRowCount { get; set; } = ComputeRowCount.OnlyInFirstPage;
    public string? Name { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Email { get; set; }
    public List<Guid> EntryIdList { get; set; } = new();

    public void GetById(Guid entryId)
    {
        IdList.Clear();
        IdList.Add(entryId);
    }

}
