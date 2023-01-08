using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record ChangeActiveBankRequest : IRequest<CommandResult>
{
	public ChangeActiveBankRequest(bool active, Guid bankId)
	{
		this.BankId = bankId;
		this.Active = active;
	}

	public Guid BankId { get; init; }
	public bool Active { get; set; }
}
