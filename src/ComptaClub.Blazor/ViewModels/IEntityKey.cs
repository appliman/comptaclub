using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Blazor.ViewModels
{
    public interface IEntityKey
    {
        public Guid Id { get; set; }
    }
}
