using ComptaClub.Contracts.Models.Entries;

namespace ComptaClub.Handlers.Entries;

internal class CreateEntryRequestHandler : IRequestHandler<CreateEntryRequest, EntryData>
{
	public Task<EntryData> Handle(CreateEntryRequest request, CancellationToken cancellationToken)
	{
		var result = new EntryData();
		result.Id = Guid.NewGuid();
		result.CreationDate = DateTime.Today.ToDayId();
		result.ValueDate = DateTime.Today.ToDayId();
		return Task.FromResult(result);
	}
}
