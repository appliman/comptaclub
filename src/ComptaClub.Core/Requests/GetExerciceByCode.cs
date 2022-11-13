using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetExerciceByCode : IRequest<Models.Exercice>
    {
        public GetExerciceByCode(string code)
        {
            this.Code = code;
        }

        public string Code { get; init; }
    }
}
