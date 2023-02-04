using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

internal class DeleteEntryRequestHandler : IRequestHandler<Requests.DeleteEntryRequest, CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public DeleteEntryRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory,
        IMediator mediator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    public async Task<CommandResult> Handle(DeleteEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entry = await db.Entries.FindAsync(request.EntryId);
        if (entry == null)
        {
            return CommandResult.CreateWarningResult("Cette ecriture n'existe pas");
        }

        var exercice = await _mediator.Send(new GetActiveExerciceRequest());
        if (exercice != null
            && entry.ExerciceId != exercice.Id)
        {
            return CommandResult.CreateInvalidResult("Vous ne pouvez pas supprimer une ecriture d'un exercice qui n'est pas actif");
        }

        entry.DeletedDate = DateTime.Today.ToDayId();

        var saveResult = await _mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry, true));

        return saveResult;
    }
}
