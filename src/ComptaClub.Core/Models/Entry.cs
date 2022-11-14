using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
    public class Entry 
    {
        public Guid Id { get; set; }
        public string PartNumber { get; set; } = null!;
        public string Label { get; set; } = null!;
        public Guid BankId { get; set; }
        public Guid ExerciceId { get; set; }
        public Guid AccountId { get; set; }
        public Guid? UserCreatorId { get; set; }
        public Guid? AssociatedMemberId { get; set; }
        public int CreationDate { get; set; }
    }
}
