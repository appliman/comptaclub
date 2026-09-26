using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Entries;

namespace ComptaClub.Handlers.Entries;

internal class GetEntryByFilterRequestHandler : IRequestHandler<GetEntryByFilterRequest, EntryData?>
{
	private readonly IComptaClubDbContextFactory _dbContextFactory;
	private readonly IMediator _mediator;

	public GetEntryByFilterRequestHandler(
		IComptaClubDbContextFactory dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<EntryData?> Handle(GetEntryByFilterRequest request, CancellationToken cancellationToken)
	{
		var page = await _mediator.Send(new GetPagedEntityListRequest<EntryListFilter, EntryData>(request.Filter));
		return page.List.SingleOrDefault();
	}
}
