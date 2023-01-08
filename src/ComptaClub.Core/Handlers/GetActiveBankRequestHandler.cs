using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class GetActiveBankRequestHandler : IRequestHandler<Requests.GetActiveBankRequest, Datas.BankData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public GetActiveBankRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
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
