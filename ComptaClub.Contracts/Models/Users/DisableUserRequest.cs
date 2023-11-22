using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Results;
using MediatR;

namespace ComptaClub.Contracts.Models.Users;

public record DisableUserRequest : IRequest<CommandResult>
{
    public DisableUserRequest(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; set; }
}
