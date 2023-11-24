using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Blazor.ViewModels
{
    public  class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsAuthenticated { get; set; } = false;
        public int RowIndex { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
