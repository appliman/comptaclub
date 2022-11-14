using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Azure;

using ComptaClub.Datas;

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
            var result = await _mediator.Send(new Requests.GetAllAccountHierarchized());
            return result;
        }

        public async Task<object> CreateOrSynchronize()
        {
            //foreach (var item in plan)
            //{
            //    var group = await _mediator.Send(new Requests.CreateAccount(item.code, item.label, item.direction));
            //    if (group != null)
            //    {
            //        var saveResult = await _mediator.Send(new Requests.SaveEntity<Models.Account>(group));
            //        if (!saveResult.HasError)
            //        {
            //            foreach (var subItem in item.children)
            //            {
            //                var account = await _mediator.Send(new Requests.CreateAccount(subItem.code, subItem.label, item.direction));
            //                account.ParentAccountId = group.Id;
            //                saveResult = await _mediator.Send(new Requests.SaveEntity<Models.Account>(account));
            //            }
            //        }
            //    }
            //}
            return null;
        }
    }
}
