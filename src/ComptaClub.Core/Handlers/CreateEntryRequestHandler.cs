using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers;

internal class CreateEntryRequestHandler : IRequestHandler<Requests.CreateEntryRequest, Datas.EntryData>
{
    public Task<Datas.EntryData> Handle(CreateEntryRequest request, CancellationToken cancellationToken)
    {
        var result = new Datas.EntryData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();
        result.ValueDate = DateTime.Today.ToDayId();    
        return Task.FromResult(result);
    }
}
