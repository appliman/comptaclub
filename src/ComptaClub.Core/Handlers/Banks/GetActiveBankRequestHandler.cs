using ComptaClub.Contracts.Models.Banks;

namespace ComptaClub.Handlers.Banks;

internal class GetActiveBankRequestHandler : IRequestHandler<GetActiveBankRequest, BankData?>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;

	public GetActiveBankRequestHandler(IComptaClubDbContextFactory dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<BankData?> Handle(GetActiveBankRequest request, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var active = await db.Banks.SingleOrDefaultAsync(i => i.Active);
		return active;
	}
}
