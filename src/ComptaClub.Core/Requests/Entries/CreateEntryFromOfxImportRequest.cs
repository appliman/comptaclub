using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests.Entries;

public record CreateEntryFromOfxImportRequest : IRequest<EntryData>
{
    public CreateEntryFromOfxImportRequest(Import.OfxTransactionImport import)
    {
        OfxTransactionImport = import;
    }

    public Import.OfxTransactionImport OfxTransactionImport { get; init; }
}
