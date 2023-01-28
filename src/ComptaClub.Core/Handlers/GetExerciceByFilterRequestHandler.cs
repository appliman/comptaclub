namespace ComptaClub.Handlers;

internal class GetExerciceByFilterRequestHandler : IRequestHandler<Requests.GetExerciceByFilterRequest, Datas.ExerciceData?>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetExerciceByFilterRequestHandler(
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Datas.ExerciceData?> Handle(Requests.GetExerciceByFilterRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Exercices.FirstOrDefaultAsync(request.Filter);

        return data;
    }
}
