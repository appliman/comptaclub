using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Models
{
	public class Balance
	{
		public Balance()
		{

		}
		public Balance(long amount)
		{
			Amount = amount;
		}
	
		public long Amount { get; init; }
	}
}
