using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record GetEntryByIdRequest : IRequest<Models.Entry>
    {
        public GetEntryByIdRequest(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}
