using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

internal class DeleteBankRequestHandler : IRequestHandler<Requests.DeleteBankRequest, Results.CommandResult>
{
	public Task<CommandResult> Handle(DeleteBankRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
