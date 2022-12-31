using System.Linq.Expressions;

namespace ComptaClub.Requests;

public record GetExerciceByFilterRequest : IRequest<Datas.ExerciceData?>
{
    public GetExerciceByFilterRequest(Expression<Func<Datas.ExerciceData, bool>> filter)
    {
        this.Filter = filter;
    }

    public Expression<Func<Datas.ExerciceData, bool>> Filter { get; init; }
}
