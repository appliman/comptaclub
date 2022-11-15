using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
    public class Exercice : IEntityKey
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Label { get; set; }
        public long InitialAmount { get; set; }

        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public int CreationDate { get; set; }
        public Guid? LastEntryId { get; set; }
        public bool Active { get; set; }
    }
}
