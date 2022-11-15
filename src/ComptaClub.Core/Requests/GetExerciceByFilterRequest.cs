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
    public record GetExerciceByFilterRequest : IRequest<Models.Exercice>
    {
        public GetExerciceByFilterRequest(Expression<Func<Datas.Exercice, bool>> filter)
        {
            this.Filter = filter;
        }

        public Expression<Func<Datas.Exercice, bool>> Filter { get; init; }
    }
}
