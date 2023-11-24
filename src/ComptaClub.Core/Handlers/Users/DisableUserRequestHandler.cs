using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Users;

internal class DisableUserRequestHandler : IRequestHandler<DisableUserRequest, CommandResult>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
	private readonly IMediator _mediator;

	public DisableUserRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}
	public async Task<CommandResult> Handle(DisableUserRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var user = await db.Users.FindAsync(request.UserId);

		if (user == null)
		{
			return CommandResult.CreateWarningResult("Cet utilisateur n'existe pas");
		}

		user.DisableDate = DateTime.Today.ToDayId();

		var saveResult = await _mediator.Send(new SaveEntityRequest<UserData>(user, true));

		return saveResult;
	}
}
