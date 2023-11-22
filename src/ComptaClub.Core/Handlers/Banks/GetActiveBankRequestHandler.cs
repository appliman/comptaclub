using ComptaClub.Contracts.Models.Banks;

namespace ComptaClub.Handlers.Banks;

internal class GetActiveBankRequestHandler : IRequestHandler<GetActiveBankRequest, BankData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public GetActiveBankRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<BankData?> Handle(GetActiveBankRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var active = await db.Banks.SingleOrDefaultAsync(i => i.Active);
		return active;
	}
}
