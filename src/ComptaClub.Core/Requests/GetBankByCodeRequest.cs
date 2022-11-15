using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetBankByCodeRequest : IRequest<Models.Bank>
    {
        public GetBankByCodeRequest(string code)
        {
            Code = code;
        }
        public string Code { get; init; } = null!;
    }
}
