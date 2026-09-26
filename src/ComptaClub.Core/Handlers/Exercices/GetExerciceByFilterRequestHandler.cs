using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Exercices;

internal class GetExerciceByFilterRequestHandler : IRequestHandler<GetExerciceByFilterRequest, ExerciceData?>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public GetExerciceByFilterRequestHandler(
        IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<ExerciceData?> Handle(GetExerciceByFilterRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Exercices.FirstOrDefaultAsync(request.Filter);

        return data;
    }
}
