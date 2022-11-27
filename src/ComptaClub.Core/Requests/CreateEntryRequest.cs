using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record CreateEntryRequest : IRequest<Datas.EntryData>
    {

    }
}
