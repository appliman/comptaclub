using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetMemberListByEntryRequestHandler : IRequestHandler<Requests.GetMemberListByEntryRequest, IEnumerable<Datas.MemberData>>
{
    private readonly IMediator _mediator;

    public GetMemberListByEntryRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IEnumerable<MemberData>> Handle(GetMemberListByEntryRequest request, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(f =>
        {
            f.EntryIdList.Add(request.EntryId);
        }));

        return page.List;
    }
}
