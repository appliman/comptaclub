using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
    public class EntryListFilter : IListFilter
    {
        public KeyIdList KeyIdList { get; } = new();
        public int PageIndex { get; set; }
        public int? Skip { get; set; }
        public int PageSize { get; set; }
        public string? SortByName { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public string? Search { get; set; }
        public ComputeRowCount ComputeRowCount { get; set; } = ComputeRowCount.OnlyInFirstPage;
        public List<Guid>? AccountIdList { get; set; }
        public List<string>? ImportIdList { get; set; }
        public Guid? ExerciceId { get; set; }
        public EntryListFilterOptions Options { get; set; } = new();
        public Enums.PaymentType? PaymentType { get; set; }

        public void GetById(Guid entryId)
        {
            KeyIdList.PropertyName = "Id";
            KeyIdList.KeyList.Add(entryId);
        }
    }
}
