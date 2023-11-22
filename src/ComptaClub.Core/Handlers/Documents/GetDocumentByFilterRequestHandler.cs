using ComptaClub.Contracts.Models.Documents;

namespace ComptaClub.Handlers.Documents;

internal class GetDocumentByFilterRequestHandler : IRequestHandler<GetDocumentByFilterRequest, DocumentData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
	private readonly IMediator _mediator;

	public GetDocumentByFilterRequestHandler(
		IDbContextFactory<ComptaClubDbContext> dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<DocumentData?> Handle(GetDocumentByFilterRequest request, CancellationToken cancellationToken)
	{
		var page = await _mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, DocumentData>(request.Filter), cancellationToken);
		return page.List.SingleOrDefault();
	}
}
