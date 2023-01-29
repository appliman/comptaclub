using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetAccountByFilterRequestHandler : IRequestHandler<Requests.GetAccountByFilterRequest, Datas.AccountData?>
{
    private readonly IMediator _mediator;

    public GetAccountByFilterRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Datas.AccountData?> Handle(Requests.GetAccountByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<Models.AccountListFilter, Datas.AccountData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
