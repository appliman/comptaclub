using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetMemberByFilterRequestHandler : IRequestHandler<Requests.GetMemberByFilterRequest, Datas.MemberData?>
{
    private readonly IMediator _mediator;

    public GetMemberByFilterRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Datas.MemberData?> Handle(Requests.GetMemberByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<Models.MemberListFilter, Datas.MemberData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
