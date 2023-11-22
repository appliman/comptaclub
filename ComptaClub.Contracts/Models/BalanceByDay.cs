using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models;

public class BalanceByDay
{
    public int DayId { get; set; }
    public decimal BalanceAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal DebitAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int DayOfMonth { get; set; }
    public DateTime Day { get; set; }
}
