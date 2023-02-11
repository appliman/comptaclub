using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;

namespace ComptaClub.Handlers.Accounts;

internal class GetPlanRequestHandler : IRequestHandler<GetPlanRequest, List<AccountData>>
{
    private readonly IMediator _mediator;

    public GetPlanRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task<List<AccountData>> Handle(GetPlanRequest request, CancellationToken cancellationToken)
    {
        var requestFilter = new GetPagedEntityListRequest<AccountListFilter, AccountData>(f =>
        {
            f.PageSize = int.MaxValue;
        });

        var page = await _mediator.Send(requestFilter, cancellationToken);

        var list = new List<AccountData>();
        foreach (var data in page.List)
        {
            data.Level = data.ParentAccountId == null ? 0 : -1;
            list.Add(data);
        }

        list.Levelize();
        list.Hierarchize();

        return list;
    }
}
