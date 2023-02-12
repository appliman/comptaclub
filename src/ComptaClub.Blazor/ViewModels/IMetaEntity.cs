using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Blazor.ViewModels
{
    public interface IMetaEntity
    {
        public int RowIndex { get; set; }
        public Guid Id { get; set; }
        public Enums.MetaEntity MetaEntity { get; }
    }
}
