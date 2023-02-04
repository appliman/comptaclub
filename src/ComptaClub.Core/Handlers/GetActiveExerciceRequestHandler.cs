using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetActiveExerciceRequestHandler : IRequestHandler<Requests.GetActiveExerciceRequest, Datas.ExerciceData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public GetActiveExerciceRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<ExerciceData?> Handle(GetActiveExerciceRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var active = await db.Exercices.SingleOrDefaultAsync(i => i.Active);
		return active;
	}
}
