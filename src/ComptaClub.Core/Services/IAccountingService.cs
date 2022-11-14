using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Services
{
	public interface IAccountingService
	{
		Task<IEnumerable<Models.Account>> GetAccountingPlan();
	}
}
