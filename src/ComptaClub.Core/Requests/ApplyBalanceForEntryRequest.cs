using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
	public record ApplyBalanceForEntryRequest : IRequest<Balance>
	{
		public ApplyBalanceForEntryRequest(Datas.EntryData entry)
		{
			this.Entry = entry;
		}

		public Datas.EntryData Entry { get; init; }
	}
}
