namespace ComptaClub.Handlers;

public class SaveEntryRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.EntryData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.EntryData> _validator;
    private readonly IMediator _mediator;

    public SaveEntryRequestHandler(
        IValidator<Datas.EntryData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveEntryRequestHandler> logger,
        MediatR.IMediator mediator)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _mediator = mediator;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.EntryData> request, CancellationToken cancellationToken)
    {
        if (!request.BypassRules)
        {
            var result = await _validator.ValidateAsync(request.Entity);
            if (!result.IsValid)
            {
                return result.ToPersistResult<Guid>()!;
            }
        }

        var saveResult = await SaveEntity<Datas.EntryData>(request.Entity);
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
