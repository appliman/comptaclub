using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Results;
using ComptaClub.Enums;

namespace ComptaClub.Handlers.Exercices;
internal class CloseExerciceRequestHandler : IRequestHandler<CloseExerciceRequest, CommandResult>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public CloseExerciceRequestHandler(IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(CloseExerciceRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var exercice = await db.Exercices.FindAsync(request.ExerciceId);
        if (exercice == null)
        {
            return CommandResult.CreateWarningResult("Cet exercice n'existe pas");
        }

        if (exercice.ExerciceState != ExerciceState.Current)
        {
            return CommandResult.CreateInvalidResult("L'exercice n'est pas en cours");
        }

        exercice.ExerciceState = ExerciceState.Closed;
        exercice.ClosedDate = DateTime.Now.ToDayId();
        db.Exercices.Attach(exercice);
        db.Entry(exercice).State = EntityState.Modified;

        var changeCount = await db.SaveChangesAsync(cancellationToken);

        return new CommandResult()
        {
            ChangeCount = changeCount,
            HasError = false
        };
    }
}
