namespace ComptaClub.Handlers;

public class SaveExerciceRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.ExerciceData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.ExerciceData> _validator;

    public SaveExerciceRequestHandler(IValidator<Datas.ExerciceData> validator, 
        IDbContextFactory<ComptaClubDbContext> dbContextFactory, 
        ILogger<SaveBankRequestHandler> logger) 
        : base(dbContextFactory, logger)
    {
        _validator = validator;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.ExerciceData> request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request.Entity);
        if (!result.IsValid)
        {
            return result.ToPersistResult<Guid>()!;
        }

        return await SaveEntity<Datas.ExerciceData>(request.Entity);
    }
}
