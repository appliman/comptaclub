using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers
{
    public class CreateEntryRequestHandler : IRequestHandler<Requests.CreateEntryRequest, Models.Entry>
    {
        public Task<Entry> Handle(CreateEntryRequest request, CancellationToken cancellationToken)
        {
            var result = new Models.Entry();
            result.Id = Guid.NewGuid();
            result.CreationDate = DateTime.Today.ToDayId();
            return Task.FromResult(result);
        }
    }
}
