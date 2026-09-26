using ComptaClub.Contracts.Models.Clubs;

namespace ComptaClub.Handlers.Clubs;
internal class GetClubRequestHandler : IRequestHandler<GetClubRequest, Datas.ClubData>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;

	public GetClubRequestHandler(IComptaClubDbContextFactory dbContextFactory)
    {
		_dbContextFactory = dbContextFactory;
	}

	public async Task<ClubData> Handle(GetClubRequest request, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var result = db.ClubDatas.SingleOrDefault();
		if (result is null)
		{
			result = new ClubData();
			result.Id = Guid.NewGuid();
			result.CreationDate = DateTime.Now.ToDayId();
		}

		return result;
	}
}
