using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
    public interface IEntityKey
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
    }
}
