using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetBankByFilterRequest : IRequest<Datas.BankData?>
    {
        public GetBankByFilterRequest(Expression<Func<Datas.BankData, bool>> filter)
        {
            Filter = filter;
        }
        public Expression<Func<Datas.BankData, bool>> Filter { get; init; } = null!;
    }
}
