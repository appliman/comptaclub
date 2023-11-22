using System.Text;

using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Import.Models;

namespace ComptaClub.Handlers.Entries;

internal class ImportEntryListFromStreamRequestHandler : IRequestHandler<ImportEntryListFromStreamRequest, IEnumerable<EntryData>>
{
	private readonly IMediator _mediator;
	private readonly ILogger<ImportEntryListFromStreamRequestHandler> _logger;

	public ImportEntryListFromStreamRequestHandler(IMediator mediator,
		ILogger<ImportEntryListFromStreamRequestHandler> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	public async Task<IEnumerable<EntryData>> Handle(ImportEntryListFromStreamRequest request, CancellationToken cancellationToken)
	{
		var content = Encoding.UTF8.GetString(request.ContentStream.GetBuffer());

		var importList = Import.OfxParser.ParseFromContent(content);

		var result = new List<EntryData>();
		foreach (var import in importList)
		{
			var entry = await _mediator.Send(new CreateEntryFromOfxImportRequest(import));
			result.Add(entry);
		}

		// On verifie si des entrées sont déjà importées
		var importIdList = result.Select(i => i.ImportId!).Distinct().ToList();
		var existingEntries = await _mediator.Send(new GetPagedEntityListRequest<EntryListFilter, EntryData>(f =>
		{
			f.PageSize = int.MaxValue;
			f.ImportIdList = importIdList;
		}));

		var existingImportList = existingEntries.List.Where(i => i.ImportId is not null).Select(i => i.ImportId!).Distinct().ToList();
		var removeCount = result.RemoveAll(i => existingImportList.Contains(i.ImportId!));
		_logger.LogTrace($"{removeCount} entrée déjà importées");

		return result;
	}
}
