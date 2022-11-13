using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
    public interface ILastUpdatable
    {
        public DateTime LastUpdate { get; set; }
    }
}
