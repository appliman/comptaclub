using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Results;

public class BalanceByMember
{
    public Guid MemberId { get; set; }
    public Guid ExerciceId { get; set; }
    public long Balance { get; set; }
    public int EntryCount { get; set; }
}
