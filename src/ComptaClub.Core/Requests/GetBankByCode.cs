using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetBankByCode : IRequest<Models.Bank>
    {
        public GetBankByCode(string code)
        {
            Code = code;
        }
        public string Code { get; init; } = null!;
    }
}
