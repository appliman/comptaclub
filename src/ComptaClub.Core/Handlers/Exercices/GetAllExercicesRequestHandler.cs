using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Requests.Exercices;

namespace ComptaClub.Handlers.Exercices;

internal class GetAllExercicesRequestHandler : IRequestHandler<GetAllExercicesRequest, List<ExerciceData>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetAllExercicesRequestHandler(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<ExerciceData>> Handle(GetAllExercicesRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var exercices = await db.Exercices.ToListAsync();
        return exercices;
    }
}
