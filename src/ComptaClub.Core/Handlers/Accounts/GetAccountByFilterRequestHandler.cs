using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class GetAccountByFilterRequestHandler : IRequestHandler<GetAccountByFilterRequest, AccountData?>
{
    private readonly IMediator _mediator;

    public GetAccountByFilterRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<AccountData?> Handle(GetAccountByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<AccountListFilter, AccountData>(request.Filter), cancellationToken);
        return page.List.SingleOrDefault();
    }
}
