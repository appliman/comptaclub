using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;

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
        public Guid? MemberId { get; set; }
        public int CreationDate { get; set; }
        public Balance? Balance { get; set; }
        public Datas.PaymentType PaymentType { get; set; }
		public AccountDirection AccountDirection { get; set; }
		public string? ExtraInfos { get; set; }
        public long Amount { get; set; }
    }
}
