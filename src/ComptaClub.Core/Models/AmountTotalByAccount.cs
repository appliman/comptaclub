using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto.Tls;

namespace ComptaClub.Models;

public class AmountTotalByAccount
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
    public long Total { get; set; }
}
