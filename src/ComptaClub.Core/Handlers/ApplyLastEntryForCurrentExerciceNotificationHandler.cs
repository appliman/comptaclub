using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Notifications;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers
{
	public class ApplyLastEntryForCurrentExerciceNotificationHandler : INotificationHandler<Notifications.EntrySavedNotification>
	{
		private readonly IMediator _mediator;
        private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

        public ApplyLastEntryForCurrentExerciceNotificationHandler(MediatR.IMediator mediator,
			IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
		{
			this._mediator = mediator;
            _dbContextFactory = dbContextFactory;
        }

		public async Task Handle(EntrySavedNotification notification, CancellationToken cancellationToken)
		{
			var exercice = await _mediator.Send(new GetExerciceByFilterRequest(i => i.Id == notification.ExerciceId))!;
			if (exercice == null) 
			{
				// Ne doit pas arriver
				return;
			}
			exercice!.LastEntryId = notification.EntryId;
			var currentBalance = await _mediator.Send(new GetCurrentBalanceRequest());
            exercice.BalanceAmount = currentBalance;
            await _mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice!));	
		}
	}
}
