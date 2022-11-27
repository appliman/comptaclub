using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetAccountByFilterRequest : IRequest<Datas.AccountData?>
    {
        public GetAccountByFilterRequest(Expression<Func<Datas.AccountData, bool>> filter)
        {
            this.Filter = filter;   
        }

        public Expression<Func<Datas.AccountData, bool>> Filter { get; init; } = null!;
    }
}
