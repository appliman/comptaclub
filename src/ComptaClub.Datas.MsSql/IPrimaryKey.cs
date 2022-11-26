using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Datas;

public interface IPrimaryKey
{
    public Guid Id { get; set; }
}
