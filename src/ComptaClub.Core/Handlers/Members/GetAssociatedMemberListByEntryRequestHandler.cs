using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

internal class GetAssociatedMemberListByEntryRequestHandler : IRequestHandler<GetAssociatedMemberListByEntryRequest, IEnumerable<AssociatedMemberListByEntryData>>
{
    private readonly IMediator _mediator;
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public GetAssociatedMemberListByEntryRequestHandler(IMediator mediator,
        IComptaClubDbContextFactory dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<AssociatedMemberListByEntryData>> Handle(GetAssociatedMemberListByEntryRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var associations = await db.AssociatedMemberListByEntries.Where(i => i.EntryId == request.EntryId).ToListAsync(cancellationToken);

        return associations;
    }
}
