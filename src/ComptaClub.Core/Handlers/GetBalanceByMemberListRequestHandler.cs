using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

using DocumentFormat.OpenXml.Vml.Office;

namespace ComptaClub.Handlers;

internal class GetBalanceByMemberListRequestHandler : IRequestHandler<Requests.GetBalanceByMemberListRequest, List<Results.BalanceByMember>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetBalanceByMemberListRequestHandler(IMediator mediator,
        IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<BalanceByMember>> Handle(GetBalanceByMemberListRequest request, CancellationToken cancellationToken)
    {
        var memberRequest = new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(request.PredicateFilter);
        if (request.Filter != null)
        {
            memberRequest = new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(request.Filter);
        }
        var memberPage = await _mediator.Send(memberRequest, cancellationToken);

        var memberIdList = memberPage.List.Select(i => i.Id).ToList();
        Guid exerciceId = request.ExerciceId.GetValueOrDefault(Guid.Empty);
        if (exerciceId == Guid.Empty)
        {
            var activeExercice = await _mediator.Send(new GetActiveExerciceRequest());
            exerciceId = activeExercice!.Id;
        }

        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = from entry in db.Entries
                    join mbe in db.AssociatedMemberListByEntries on entry.Id equals mbe.EntryId
                    where memberIdList.Contains(mbe.MemberId)
                        && !entry.DeletedDate.HasValue
                        && entry.ExerciceId == exerciceId
                    group new { entry, mbe } by mbe.MemberId into g
                    select new Results.BalanceByMember
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
