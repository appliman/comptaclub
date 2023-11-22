using ComptaClub.Contracts.Models.Users;

namespace ComptaClub.Handlers.Users;

internal class GetUserByFilterRequestHandler : IRequestHandler<GetUserByFilterRequest, UserData?>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
	private readonly IMediator _mediator;

	public GetUserByFilterRequestHandler(
		IDbContextFactory<ComptaClubDbContext> dbContextFactory,
		IMediator mediator)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
	}

	public async Task<UserData?> Handle(GetUserByFilterRequest request, CancellationToken cancellationToken)
	{
		var page = await _mediator.Send(new GetPagedEntityListRequest<UserListFilter, UserData>(request.Filter));
		return page.List.SingleOrDefault();
	}
}
