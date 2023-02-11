using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models;

public class DocumentListFilter : IListFilter
{
    public KeyIdList KeyIdList { get; } = new();
    public int PageIndex { get; set; }
    public int? Skip { get; set; }
    public int PageSize { get; set; }
    public string? SortByName { get; set; }
    public ListSortDirection SortDirection { get; set; }
    public string? Search { get; set; }
    public ComputeRowCount ComputeRowCount { get; set; } = ComputeRowCount.OnlyInFirstPage;

    public void GetById(Guid id)
    {
        KeyIdList.PropertyName = "Id";
        KeyIdList.KeyList.Add(id);
    }

}
