using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Handlers.Exercices;
internal class GetPeriodListFromExerciceRequestHandler : IRequestHandler<GetPeriodListFromExerciceRequest, List<PeriodFilter>>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public GetPeriodListFromExerciceRequestHandler(IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<PeriodFilter>> Handle(GetPeriodListFromExerciceRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var result = new List<PeriodFilter>();
        var exercice = await db.Exercices.FindAsync(request.ExerciceId);
        if (exercice is null)
        {
            return result;
        }

        var date = exercice.StartDate.FromDayId();
        date = new DateTime(date.Year, date.Month, 1);
        int loop = 1;
        while (loop < 13)
        {
            var period = new PeriodFilter
            {
                Name = date.ToString("MMMM yyyy"),
                FromDayId = date.ToDayId(),
                ToDayId = date.AddMonths(1).AddDays(-1).ToDayId()
            };
            date = date.AddMonths(1);
            result.Add(period);
            loop++;
        }

        return result;
    }
}
