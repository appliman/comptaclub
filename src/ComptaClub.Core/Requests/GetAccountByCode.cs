using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetAccountByCode : IRequest<Models.Account>
    {
        public GetAccountByCode(string code)
        {
            this.Code = code;   
        }

        public string Code { get; init; } = null!;
    }
}
