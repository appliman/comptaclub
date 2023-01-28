using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

internal class DisableUserRequestHandler : IRequestHandler<Requests.DisableUserRequest, Results.CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public DisableUserRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory,
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

        var saveResult = await _mediator.Send(new SaveEntityRequest<Datas.UserData>(user, true));

        return saveResult;
    }
}
