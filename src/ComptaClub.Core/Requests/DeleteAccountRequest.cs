using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record DeleteAccountRequest : IRequest<CommandResult>
{
	public DeleteAccountRequest(Guid accountId)
	{
		this.AccountId = accountId;
	}

	public Guid AccountId { get; init; }
}
