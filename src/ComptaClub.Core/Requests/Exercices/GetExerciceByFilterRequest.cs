using System.Linq.Expressions;

namespace ComptaClub.Requests.Exercices;

public record GetExerciceByFilterRequest : IRequest<ExerciceData?>
{
    public GetExerciceByFilterRequest(Expression<Func<ExerciceData, bool>> filter)
    {
        Filter = filter;
    }

    public Expression<Func<ExerciceData, bool>> Filter { get; init; }
}
