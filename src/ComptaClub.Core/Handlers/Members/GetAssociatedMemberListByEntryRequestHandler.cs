using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

internal class GetAssociatedMemberListByEntryRequestHandler : IRequestHandler<GetAssociatedMemberListByEntryRequest, IEnumerable<AssociatedMemberListByEntryData>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAssociatedMemberListByEntryRequestHandler(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<AssociatedMemberListByEntryData>> Handle(GetAssociatedMemberListByEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var associations = await db.AssociatedMemberListByEntries.Where(i => i.EntryId == request.EntryId).ToListAsync(cancellationToken);

        return associations;
    }
}
