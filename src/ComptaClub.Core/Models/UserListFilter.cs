using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models;

public class UserListFilter : IListFilter
{
    public KeyIdList KeyIdList { get; } = new();
    public int PageIndex { get; set; }
    public int? Skip { get; set; }
    public int PageSize { get; set; }
    public string? SortByName { get; set; }
    public ListSortDirection SortDirection { get; set; }
    public string? Search { get; set; }
    public ComputeRowCount ComputeRowCount { get; set; } = ComputeRowCount.OnlyInFirstPage;
    public UserListFilterOptions Options { get; set; } = new();
    public string? Email { get; set; }

    public void GetById(Guid entryId)
    {
        KeyIdList.PropertyName = "Id";
        KeyIdList.KeyList.Add(entryId);
    }

}
