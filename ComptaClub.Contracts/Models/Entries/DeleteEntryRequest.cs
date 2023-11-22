using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Entries;

public record DeleteEntryRequest : IRequest<CommandResult>
{
    public DeleteEntryRequest(Guid entryId)
    {
        EntryId = entryId;
    }

    public Guid EntryId { get; init; }
}
