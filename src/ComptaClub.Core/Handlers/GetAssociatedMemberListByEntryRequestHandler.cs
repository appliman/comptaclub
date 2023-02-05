using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetAssociatedMemberListByEntryRequestHandler : IRequestHandler<Requests.GetAssociatedMemberListByEntryRequest, IEnumerable<Datas.AssociatedMemberListByEntryData>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAssociatedMemberListByEntryRequestHandler(IMediator mediator,
        IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Datas.AssociatedMemberListByEntryData>> Handle(GetAssociatedMemberListByEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var associations = await db.AssociatedMemberListByEntries.Where(i => i.EntryId == request.EntryId).ToListAsync(cancellationToken);
        
        return associations;
    }
}
