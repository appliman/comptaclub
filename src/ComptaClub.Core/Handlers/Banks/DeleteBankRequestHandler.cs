using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Banks;

internal class DeleteBankRequestHandler : IRequestHandler<DeleteBankRequest, CommandResult>
{
    public Task<CommandResult> Handle(DeleteBankRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
