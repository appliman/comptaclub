namespace ComptaClub.Handlers;

internal class SaveAccountRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.AccountData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.AccountData> _validator;

    public SaveAccountRequestHandler(
        IValidator<Datas.AccountData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveAccountRequestHandler> logger)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.AccountData> request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request.Entity);
        if (!result.IsValid)
        {
            return result.ToPersistResult<Guid>()!;
        }

        return await SaveEntity<Datas.AccountData>(request.Entity);
    }
}
