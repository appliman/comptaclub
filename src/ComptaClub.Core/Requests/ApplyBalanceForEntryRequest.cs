using System;
using System.Collections.Generic;

namespace ComptaClub.Requests;

public record ApplyBalanceForEntryRequest : IRequest<Balance>
{
	public ApplyBalanceForEntryRequest(Datas.EntryData entry)
	{
		this.Entry = entry;
	}

	public Datas.EntryData Entry { get; init; }
}
