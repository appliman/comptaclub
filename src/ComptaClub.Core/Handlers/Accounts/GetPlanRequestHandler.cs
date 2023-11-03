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
        var list = (await _mediator.GetAllAccounts()).ToList();
        list.Levelize();
        list.Hierarchize();

        return list;
    }
}
