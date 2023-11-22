namespace ComptaClub.Contracts.Models.Members;

public record GetBalanceByMemberListRequest : IRequest<List<BalanceByMember>>
{
	public GetBalanceByMemberListRequest(Action<MemberListFilter> predicateFilter, Guid? exerciceId = null)
	{
		PredicateFilter = predicateFilter;
		ExerciceId = exerciceId;
	}

	public GetBalanceByMemberListRequest(MemberListFilter filter, Guid? exerciceId = null)
	{
		Filter = filter;
		ExerciceId = exerciceId;
	}

	public MemberListFilter? Filter { get; init; }

	public Action<MemberListFilter>? PredicateFilter { get; init; }
	public Guid? ExerciceId { get; init; }
}
