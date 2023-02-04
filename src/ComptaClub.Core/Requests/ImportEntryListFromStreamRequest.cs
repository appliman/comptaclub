using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Requests;

public record ImportEntryListFromStreamRequest : IRequest<IEnumerable<Datas.EntryData>>
{
	public ImportEntryListFromStreamRequest(MemoryStream contentStream)
	{
		this.ContentStream = contentStream;
	}

	public MemoryStream ContentStream { get; init; }
}
