using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class ImportEntryListFromStreamRequestHandler : IRequestHandler<Requests.ImportEntryListFromStreamRequest, IEnumerable<Datas.EntryData>>
{
    private readonly IMediator _mediator;

    public ImportEntryListFromStreamRequestHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IEnumerable<EntryData>> Handle(ImportEntryListFromStreamRequest request, CancellationToken cancellationToken)
    {
        var content = System.Text.Encoding.UTF8.GetString(request.ContentStream.GetBuffer());

        var importList = Import.OfxParser.ParseFromContent(content);

        var result = new List<EntryData>();
        foreach (var import in importList)
        {
            var entry = await _mediator.Send(new CreateEntryFromOfxImportRequest(import));
            result.Add(entry);
        }
        return result;
    }
}
