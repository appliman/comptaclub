using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests.Accounts;

public record DeleteAccountRequest : IRequest<CommandResult>
{
    public DeleteAccountRequest(Guid accountId)
    {
        AccountId = accountId;
    }

    public Guid AccountId { get; init; }
}
