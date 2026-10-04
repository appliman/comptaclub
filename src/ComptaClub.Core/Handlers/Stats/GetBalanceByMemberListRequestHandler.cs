using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Stats;

internal class GetBalanceByMemberListRequestHandler : IRequestHandler<GetBalanceByMemberListRequest, List<BalanceByMember>>
{
	private readonly IMediator _mediator;
	private readonly IComptaClubDbContextFactory _dbContextFactory;

	public GetBalanceByMemberListRequestHandler(IMediator mediator,
		IComptaClubDbContextFactory dbContextFactory)
	{
		_mediator = mediator;
		_dbContextFactory = dbContextFactory;
	}

	public async Task<List<BalanceByMember>> Handle(GetBalanceByMemberListRequest request, CancellationToken cancellationToken)
	{
		var memberRequest = new GetPagedEntityListRequest<MemberListFilter, MemberData>(request.PredicateFilter);
		if (request.Filter != null)
		{
			memberRequest = new GetPagedEntityListRequest<MemberListFilter, MemberData>(request.Filter);
		}
		var memberPage = await _mediator.Send(memberRequest, cancellationToken);

		var memberIdList = memberPage.List.Select(i => i.Id).ToList();
		Guid exerciceId = request.ExerciceId.GetValueOrDefault(Guid.Empty);
		if (exerciceId == Guid.Empty)
		{
            var activeExercice = await _mediator.Send(new GetActiveExerciceRequest(), cancellationToken);
            if (activeExercice is null)
            {
                return [];
            }
            exerciceId = activeExercice.Id;
		}

		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var query = from entry in db.Entries
					join mbe in db.AssociatedMemberListByEntries on entry.Id equals mbe.EntryId
					where memberIdList.Contains(mbe.MemberId)
						&& !entry.DeletedDate.HasValue
						&& entry.ExerciceId == exerciceId
					group new { entry, mbe } by mbe.MemberId into g
					select new BalanceByMember
					{
						ExerciceId = exerciceId,
						MemberId = g.Key,
						Balance = g.Sum(i => i.mbe.Amount * (int)i.entry.AccountDirection),
						EntryCount = g.Select(i => i.entry.Id).Distinct().Count()
					};

		var list = await query.ToListAsync();
		return list;

	}
}
