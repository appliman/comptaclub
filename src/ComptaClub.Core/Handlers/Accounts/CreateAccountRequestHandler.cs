using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Models;

using MediatR;
using ComptaClub.Requests.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class CreateAccountRequestHandler : IRequestHandler<CreateAccountRequest, AccountData>
{
    public Task<AccountData> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = new AccountData();
        result.Id = Guid.NewGuid();
        result.Code = request.Code;
        result.Label = request.Label;
        result.Direction = request.Direction;
        result.CreationDate = DateTime.Today.ToDayId();
        result.ParentAccountId = request.ParentId;
        return Task.FromResult(result);
    }
}
