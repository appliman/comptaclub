using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Notifications
{
	public record EntrySavedNotification : INotification
	{
		public Guid EntryId { get; set; }
		public Guid ExerciceId { get; set; }
		public long BalanceAmount { get; set; }
	}
}
