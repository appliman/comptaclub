using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;

using MediatR;

namespace ComptaClub.Requests
{
	public record ApplyBalanceForEntryRequest : IRequest<Balance>
	{
		public ApplyBalanceForEntryRequest(Models.Entry entry)
		{
			this.Entry = entry;
		}

		public Models.Entry Entry { get; init; }
	}
}
