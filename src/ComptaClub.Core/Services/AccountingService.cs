using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Azure;

using MediatR;

using static Azure.Core.HttpHeader;

namespace ComptaClub.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly IMediator _mediator;

        public AccountingService(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<Models.Account>> GetAccountingPlan()
        {
            var result = await _mediator.Send(new Requests.GetAllAccountHierarchizedRequest());
            return result;
        }

    }
}
