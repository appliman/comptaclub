using System;
using System.Collections.Generic;
using System.Text;

namespace ComptaClub.Contracts.Models;

public interface IListFilter
{
    List<Guid> IdList { get; set; }
    int PageIndex { get; set; }
    int? Skip { get; set; }
    int PageSize { get; set; }
    string? SortByName { get; set; }
    System.ComponentModel.ListSortDirection SortDirection { get; set; }
    string? Search { get; set; }
    ComputeRowCount ComputeRowCount { get; set; }
}
