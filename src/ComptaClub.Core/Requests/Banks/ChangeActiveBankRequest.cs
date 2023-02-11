using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests.Banks;

public record ChangeActiveBankRequest : IRequest<CommandResult>
{
    public ChangeActiveBankRequest(bool active, Guid bankId)
    {
        BankId = bankId;
        Active = active;
    }

    public Guid BankId { get; init; }
    public bool Active { get; set; }
}
