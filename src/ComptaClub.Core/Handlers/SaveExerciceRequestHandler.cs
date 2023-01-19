using ComptaClub.Requests;

namespace ComptaClub.Handlers;

public class SaveExerciceRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.SaveEntityRequest<Datas.ExerciceData>, Results.PersistResult<Guid>>
{
    private readonly IValidator<Datas.ExerciceData> _validator;
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;

    public SaveExerciceRequestHandler(IValidator<Datas.ExerciceData> validator, 
        IDbContextFactory<ComptaClubDbContext> dbContextFactory, 
        ILogger<SaveBankRequestHandler> logger,
        IMediator mediator) 
        : base(dbContextFactory, logger)
    {
        _validator = validator;
		_dbContextFactory = dbContextFactory;
        _mediator = mediator;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.SaveEntityRequest<Datas.ExerciceData> request, CancellationToken cancellationToken)
    {
        var valid = await _validator.ValidateAsync(request.Entity);
        if (!valid.IsValid)
        {
            return valid.ToPersistResult<Guid>()!;
        }

        var db = await _dbContextFactory.CreateDbContextAsync();
        var exerciceCount = await db.Exercices.CountAsync();

        // S'il n'y a aucun exercice, le nouveau doit etre actif
        if (exerciceCount == 0)
        {
            request.Entity.Active = true;
        }

        if (request.Entity.Active)
        {
            var balance = await _mediator.Send(new GetCurrentBalanceRequest());
            request.Entity.BalanceAmount = balance;
        }

        var result = await SaveEntity<Datas.ExerciceData>(request.Entity);
        if (!result.HasError)
        {
			exerciceCount = await db.Exercices.CountAsync();
            // Si c'est le seul exercice, il doit etre actif impérativement
            if (exerciceCount == 1
                && !request.Entity.Active)
            {
                request.Entity.Active = true;
				result = await SaveEntity<Datas.ExerciceData>(request.Entity);
			}
		}




        return result;
    }
}
