using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class GetPlanRequestHandler : IRequestHandler<GetPlanRequest, List<Datas.AccountData>>
{
    private readonly IMediator _mediator;

    public GetPlanRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task<List<AccountData>> Handle(GetPlanRequest request, CancellationToken cancellationToken)
    {
        var requestFilter = new GetPagedEntityListRequest<Models.AccountListFilter, Datas.AccountData>(f => 
        {
            f.PageSize = int.MaxValue;
        });

		var page = await _mediator.Send(requestFilter, cancellationToken);

        var list = new List<Datas.AccountData>();
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
