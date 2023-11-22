using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
