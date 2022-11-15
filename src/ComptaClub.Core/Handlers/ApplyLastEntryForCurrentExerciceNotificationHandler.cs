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

		public ApplyLastEntryForCurrentExerciceNotificationHandler(MediatR.IMediator mediator)
		{
			this._mediator = mediator;
		}

		public async Task Handle(EntrySavedNotification notification, CancellationToken cancellationToken)
		{
			var exercice = await _mediator.Send(new GetExerciceByFilterRequest(i => i.RowKey == $"{notification.ExerciceId}"));
			if (exercice == null) 
			{ 
				// TODO Log
			}
			exercice!.LastEntryId = notification.EntryId;
			await _mediator.Send(new SaveEntityRequest<Models.Exercice>(exercice!));	
		}
	}
}
