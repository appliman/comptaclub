using ComptaClub.Datas;

using ChannelMediator;

namespace ComptaClub.Import.Models;

public record CreateEntryFromOfxImportRequest : IRequest<EntryData>
{
	public CreateEntryFromOfxImportRequest(OfxTransactionImport import)
	{
		OfxTransactionImport = import;
	}

	public OfxTransactionImport OfxTransactionImport { get; init; }
}
