using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models
{
    public class EnumExtension<E>
    {
        public E Key { get; set; } = default!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
