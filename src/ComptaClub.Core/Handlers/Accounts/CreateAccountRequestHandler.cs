using ComptaClub.Contracts.Models.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class CreateAccountRequestHandler : IRequestHandler<CreateAccountRequest, AccountData>
{
	public Task<AccountData> Handle(CreateAccountRequest request, CancellationToken cancellationToken)
	{
		var result = new AccountData();
		result.Id = Guid.NewGuid();
		result.Code = request.Code;
		result.Label = request.Label;
		result.Direction = request.Direction;
		result.CreationDate = DateTime.Today.ToDayId();
		result.ParentAccountId = request.ParentId;
		return Task.FromResult(result);
	}
}
