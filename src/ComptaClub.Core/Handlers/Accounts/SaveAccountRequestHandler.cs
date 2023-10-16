using Azure.Core;

using ComptaClub.Requests;

namespace ComptaClub.Handlers.Accounts;

internal class SaveAccountRequestHandler : SaveRequestHandlerBase, IRequestHandler<SaveEntityRequest<AccountData>, Results.PersistResult>
{
    private readonly IValidator<AccountData> _validator;
    private readonly IMediator _mediator;

    public SaveAccountRequestHandler(
        IValidator<AccountData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveAccountRequestHandler> logger,
        IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<Results.PersistResult> Handle(SaveEntityRequest<AccountData> request, CancellationToken cancellationToken)
    {
        var existing = await _mediator.Send(new Requests.Accounts.GetAccountByFilterRequest(f => f.GetById(request.Entity.Id)), cancellationToken);
        if (existing != null
            && existing.Direction != request.Entity.Direction)
        {
            // Changer la direction de tous les enfants
            await ChangeDirectionForAllChildren(request.Entity, cancellationToken);
        }

        var result = await _validator.ValidateAsync(request.Entity, cancellationToken);
        if (!result.IsValid)
        {
            return result.ToPersistResult()!;
        }

        return await SaveEntity<AccountData>(request.Entity, cancellationToken);
    }

    private async Task ChangeDirectionForAllChildren(AccountData root, CancellationToken cancellationToken)
    {
        var children = await _mediator.Send(new GetPagedEntityListRequest<AccountListFilter, AccountData>(f => f.ParentAccountId = root.Id), cancellationToken);
        if (children == null
            || children.List == null
            || !children.List.Any())
        {
            return;
        }
        foreach (var account in children.List)
        {
            account.Direction = root.Direction;
            await SaveEntity<AccountData>(account,cancellationToken);
            await ChangeDirectionForAllChildren(account, cancellationToken);
        }
    }
}
