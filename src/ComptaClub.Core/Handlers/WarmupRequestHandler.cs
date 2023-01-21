using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Configuration;
using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class WarmupRequestHandler : IRequestHandler<Requests.WarmupRequest, Results.CommandResult>
{
	private readonly IMediator _mediator;
	private readonly ComptaClubSettings _settings;

	public WarmupRequestHandler(IMediator mediator,
		Configuration.ComptaClubSettings settings)
	{
		_mediator = mediator;
		_settings = settings;
	}

	async Task<CommandResult> IRequestHandler<WarmupRequest, CommandResult>.Handle(WarmupRequest request, CancellationToken cancellationToken)
	{
		var defaultUser = await _mediator.Send(new GetUserByFilterRequest(i => i.Email = _settings.AdminUserEmail));
		if (defaultUser != null)
		{
			return new CommandResult();
		}

		defaultUser = await _mediator.Send(new CreateUserRequest(_settings.AdminUserEmail!, _settings.AdminUserEmail!));

		var saveResult = await _mediator.Send(new SaveEntityRequest<Datas.UserData>(defaultUser));
		return saveResult;
	}
}
