using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class GetEntryByFilterRequestHandler : IRequestHandler<Requests.GetEntryByFilterRequest, Datas.EntryData?>
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

    public async Task<Datas.EntryData?> Handle(Requests.GetEntryByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
