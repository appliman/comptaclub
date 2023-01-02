using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Blazor.ViewModels
{
    public class Exercice : IEntityKey
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Label { get; set; }
        public decimal InitialAmount { get; set; }
		public decimal BalanceAmount { get; set; }

		public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public Guid? LastEntryId { get; set; }
        public bool Active { get; set; }
    }
}
