using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record CreateEntryFromOfxImportRequest : IRequest<Datas.EntryData>
{
	public CreateEntryFromOfxImportRequest(Import.OfxTransactionImport import)
	{
		this.OfxTransactionImport = import;
	}

	public Import.OfxTransactionImport OfxTransactionImport { get; init; }
}
