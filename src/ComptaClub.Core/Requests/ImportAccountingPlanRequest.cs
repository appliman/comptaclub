using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
	public record ImportAccountingPlanRequest : IRequest<Models.CommandResult>
	{
		public ImportAccountingPlanRequest(List<Models.Account> plan)
		{
			this.HierarchizedAccountingPlan = plan;
		}

		public List<Models.Account> HierarchizedAccountingPlan { get; init; }
	}
}
