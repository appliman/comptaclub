using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record DeleteBankRequest : IRequest<CommandResult>
{
	public DeleteBankRequest(Guid bankId)
	{
		this.BankId = bankId;
	}

	public Guid BankId { get; init; }
}
