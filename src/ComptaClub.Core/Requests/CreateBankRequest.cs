using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record CreateBankRequest : IRequest<Datas.BankData>
    {
        public CreateBankRequest(string code, string label)
        {
            Code = code;
            Label = label;
        }

        public string Code { get; init; } = null!;
        public string Label { get; init; } = null!;
    }
}
