using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

using MediatR;

namespace ComptaClub.Requests;

public record DisableUserRequest : IRequest<CommandResult>
{
	public DisableUserRequest(Guid userId)
	{
		this.UserId = userId;
	}

	public Guid UserId { get; set; }
}
