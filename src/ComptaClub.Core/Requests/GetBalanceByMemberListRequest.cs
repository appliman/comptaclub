using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Results;

namespace ComptaClub.Requests;

public record GetBalanceByMemberListRequest : IRequest<List<BalanceByMember>>
{
	public GetBalanceByMemberListRequest(Action<MemberListFilter> predicateFilter, Guid? exerciceId = null)
	{
        this.PredicateFilter = predicateFilter;
        this.ExerciceId = exerciceId;
    }

    public GetBalanceByMemberListRequest(MemberListFilter filter, Guid? exerciceId = null)
    {
        this.Filter = filter;
        this.ExerciceId = exerciceId;
    }

    public MemberListFilter? Filter { get; init; }

    public Action<MemberListFilter>? PredicateFilter { get; init; }
    public Guid? ExerciceId { get; init; }
}
