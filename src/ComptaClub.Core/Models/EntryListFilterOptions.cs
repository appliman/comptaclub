using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models;

public class EntryListFilterOptions
{
    public DeletedState DeletedState { get; set; } = DeletedState.Undeleted;
    public bool WithAssociatedMemberInfo { get; set; } = false;
}
