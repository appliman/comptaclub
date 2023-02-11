using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Notifications;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers.Entries;

internal class ApplyLastEntryForCurrentExerciceNotificationHandler : INotificationHandler<EntrySavedNotification>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public ApplyLastEntryForCurrentExerciceNotificationHandler(IMediator mediator,
    IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task Handle(EntrySavedNotification notification, CancellationToken cancellationToken)
    {
        var exercice = await _mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(i => i.Id == notification.ExerciceId))!;
        if (exercice == null)
        {
            // Ne doit pas arriver
            return;
        }
        exercice!.LastEntryId = notification.EntryId;
        var currentBalance = await _mediator.Send(new Requests.Stats.GetCurrentBalanceRequest());
        exercice.BalanceAmount = currentBalance;
        await _mediator.Send(new SaveEntityRequest<ExerciceData>(exercice!));
    }
}
