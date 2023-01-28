using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetUserByFilterRequestHandler : IRequestHandler<Requests.GetUserByFilterRequest, Datas.UserData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public GetUserByFilterRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        IMediator mediator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    public async Task<Datas.UserData?> Handle(Requests.GetUserByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<Models.UserListFilter, Datas.UserData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
