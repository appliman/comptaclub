using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Models.Documents;

namespace ComptaClub.Handlers.Documents;

internal class CreateDocumentRequestHandler : IRequestHandler<CreateDocumentRequest, Datas.DocumentData>
{
    public Task<DocumentData> Handle(CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = new DocumentData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();
        result.LastUpdate = DateTime.Today.ToDayId();

        return Task.FromResult(result);
    }
}
