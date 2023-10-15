namespace ComptaClub.Handlers.Entries;

internal class SaveEntryRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<EntryData>, Results.PersistResult>
{
    private readonly IValidator<EntryData> _validator;
    private readonly IMediator _mediator;

    public SaveEntryRequestHandler(
        IValidator<EntryData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveEntryRequestHandler> logger,
        IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<Results.PersistResult> Handle(Requests.SaveEntityRequest<EntryData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity, cancellationToken);
            if (!result.IsValid)
            {
                return result.ToPersistResult()!;
            }
        }

        var saveResult = await SaveEntity<EntryData>(request.Entity, cancellationToken);
        if (!saveResult.HasError)
        {
            await _mediator.Publish(new Notifications.EntrySavedNotification()
            {
                EntryId = request.Entity.Id,
                ExerciceId = request.Entity.ExerciceId,
            });
        }
        return saveResult;
    }
}
