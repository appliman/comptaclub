using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Banks;

public record ChangeActiveBankRequest : IRequest<CommandResult>
{
	public ChangeActiveBankRequest(bool active, Guid bankId)
	{
		BankId = bankId;
		Active = active;
	}

	public Guid BankId { get; init; }
	public bool Active { get; set; }
}
