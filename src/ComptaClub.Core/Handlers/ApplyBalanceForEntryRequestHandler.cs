using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;
using ComptaClub.Services;

using MediatR;

namespace ComptaClub.Handlers
{
	public class ApplyBalanceForEntryRequestHandler : IRequestHandler<Requests.ApplyBalanceForEntryRequest, Models.Balance>
	{
		private readonly IMediator _mediator;

		public ApplyBalanceForEntryRequestHandler(MediatR.IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<Balance> Handle(ApplyBalanceForEntryRequest request, CancellationToken cancellationToken)
		{
			var currentBalance = await _mediator.Send(new GetCurrentBalanceRequest());
			if (currentBalance == null)
			{
				throw new Exception("Balance is not applicable");
			}

			var direction = request.Entry.AccountDirection == AccountDirection.Debit ? -1 : 1;
			var balance = new Balance(currentBalance.Amount + (request.Entry.Amount * direction));
			request.Entry.Balance = balance;
			return balance;
		}
	}
}
