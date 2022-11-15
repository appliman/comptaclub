using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetAccountByCodeRequest : IRequest<Models.Account>
    {
        public GetAccountByCodeRequest(string code)
        {
            this.Code = code;   
        }

        public string Code { get; init; } = null!;
    }
}
