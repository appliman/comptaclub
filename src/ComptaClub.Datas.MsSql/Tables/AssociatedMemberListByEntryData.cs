using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Datas;

[Table("AssociatedMemberListByEntries")]
public class AssociatedMemberListByEntryData
{
    [Key]
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid EntryId { get; set; }
    public int CreationDate { get; set; }
}
