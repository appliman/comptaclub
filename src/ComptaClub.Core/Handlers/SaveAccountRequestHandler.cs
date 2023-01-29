using Azure.Core;

using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class SaveAccountRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.AccountData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.AccountData> _validator;
    private readonly IMediator _mediator;

    public SaveAccountRequestHandler(
        IValidator<Datas.AccountData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveAccountRequestHandler> logger,
        IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.AccountData> request, CancellationToken cancellationToken)
    {
        var existing = await _mediator.Send(new GetAccountByFilterRequest(f => f.GetById(request.Entity.Id)));
        if (existing != null
            && existing.Direction != request.Entity.Direction)
        {
            // Changer la direction de tous les enfants
            await ChangeDirectionForAllChildren(request.Entity);
        }

        var result = await _validator.ValidateAsync(request.Entity);
        if (!result.IsValid)
        {
            return result.ToPersistResult<Guid>()!;
        }

        return await SaveEntity<Datas.AccountData>(request.Entity);
    }

    private async Task ChangeDirectionForAllChildren(Datas.AccountData root)
    {
        var children = await _mediator.Send(new GetPagedEntityListRequest<AccountListFilter, Datas.AccountData>(f => f.ParentAccountId = root.Id));
        if (children == null
            || children.List == null
            || !children.List.Any())
        {
            return;
        }
        foreach (var account in children.List)
        {
            account.Direction = root.Direction;
            await SaveEntity<Datas.AccountData>(account);
            await ChangeDirectionForAllChildren(account);
        }
    }
}
