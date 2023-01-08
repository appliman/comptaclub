namespace ComptaClub.Handlers
{
	public class ApplyBalanceForEntryRequestHandler : IRequestHandler<Requests.ApplyBalanceForEntryRequest, long>
	{
		private readonly IMediator _mediator;

		public ApplyBalanceForEntryRequestHandler(MediatR.IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<long> Handle(Requests.ApplyBalanceForEntryRequest request, CancellationToken cancellationToken)
		{
			var currentBalance = await _mediator.Send(new Requests.GetCurrentBalanceRequest());
			if (!currentBalance.HasValue)
			{
				throw new Exception("Balance is not applicable");
			}

			var balance = currentBalance.Value + (Math.Abs(request.Entry.Amount) * (int)request.Entry.AccountDirection);
			request.Entry.BalanceValue = balance;
			return balance;
		}
	}
}
