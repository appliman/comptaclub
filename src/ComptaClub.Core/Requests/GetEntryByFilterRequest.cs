using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetEntryByFilterRequest : IRequest<Datas.EntryData?>
    {
        public GetEntryByFilterRequest(Expression<Func<Datas.EntryData, bool>> filter)
        {
            this.Filter = filter;
        }

        public Expression<Func<Datas.EntryData, bool>> Filter { get; init; }
    }
}
