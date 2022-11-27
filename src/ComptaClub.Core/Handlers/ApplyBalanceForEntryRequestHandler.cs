namespace ComptaClub.Handlers
{
	public class ApplyBalanceForEntryRequestHandler : IRequestHandler<Requests.ApplyBalanceForEntryRequest, Models.Balance>
	{
		private readonly IMediator _mediator;

		public ApplyBalanceForEntryRequestHandler(MediatR.IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<Balance> Handle(Requests.ApplyBalanceForEntryRequest request, CancellationToken cancellationToken)
		{
			var currentBalance = await _mediator.Send(new Requests.GetCurrentBalanceRequest());
			if (currentBalance == null)
			{
				throw new Exception("Balance is not applicable");
			}

			var direction = request.Entry.AccountDirection == AccountDirection.Debit ? -1 : 1;
			var balance = new Balance(currentBalance.Amount + (request.Entry.Amount * direction));
			request.Entry.BalanceValue = balance.Amount;
			return balance;
		}
	}
}
