using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Banks;

public record DeleteBankRequest : IRequest<CommandResult>
{
	public DeleteBankRequest(Guid bankId)
	{
		BankId = bankId;
	}

	public Guid BankId { get; init; }
}
