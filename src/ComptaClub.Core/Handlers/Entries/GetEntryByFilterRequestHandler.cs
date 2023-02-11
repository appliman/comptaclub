using ComptaClub.Requests;
using ComptaClub.Requests.Entries;

namespace ComptaClub.Handlers.Entries;

internal class GetEntryByFilterRequestHandler : IRequestHandler<GetEntryByFilterRequest, EntryData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public GetEntryByFilterRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        IMediator mediator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    public async Task<EntryData?> Handle(GetEntryByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<EntryListFilter, EntryData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
