using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComptaClub.Contracts.Models
{
    public class PagedList<TList>
        where TList : IEnumerable
    {
        public TList List { get; set; } = default!;
        public PagedTotal Total { get; set; } = new();
    }
}
