using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models.Entries;

public record ImportEntryListFromStreamRequest : IRequest<IEnumerable<EntryData>>
{
    public ImportEntryListFromStreamRequest(MemoryStream contentStream)
    {
        ContentStream = contentStream;
    }

    public MemoryStream ContentStream { get; init; }
}
