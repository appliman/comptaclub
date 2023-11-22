using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Clubs;
internal class SaveClubRequestHandler : IRequestHandler<SaveClubRequest, PersistResult>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

	public SaveClubRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	public async Task<PersistResult> Handle(SaveClubRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var existing = await db.ClubDatas.FindAsync(request.Club.Id);
		if (existing is null)
		{
			db.ClubDatas.Add(request.Club);
			db.Entry(request.Club).State = EntityState.Added;
		}
		else
		{
			db.ClubDatas.Attach(request.Club);
			db.Entry(request.Club).State = EntityState.Modified;
		}

		var changeCount = await db.SaveChangesAsync(cancellationToken);

		return new PersistResult()
		{
			ChangeCount = changeCount,
			Id = request.Club.Id
		};
	}
}
