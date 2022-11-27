using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetUserByFilterRequest : IRequest<Datas.UserData?>
    {
        public GetUserByFilterRequest(Expression<Func<Datas.UserData, bool>> filter)
        {
            this.Filter = filter;
        }

        public Expression<Func<Datas.UserData, bool>> Filter { get; init; }
    }
}
