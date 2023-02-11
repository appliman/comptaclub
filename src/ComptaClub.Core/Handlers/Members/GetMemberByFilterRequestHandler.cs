using ComptaClub.Requests;
using ComptaClub.Requests.Members;

namespace ComptaClub.Handlers.Members;

internal class GetMemberByFilterRequestHandler : IRequestHandler<GetMemberByFilterRequest, MemberData?>
{
    private readonly IMediator _mediator;

    public GetMemberByFilterRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<MemberData?> Handle(GetMemberByFilterRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<MemberListFilter, MemberData>(request.Filter));
        return page.List.SingleOrDefault();
    }
}
