using ComptaClub.Contracts.Models;

namespace ComptaClub.Contracts.Models.Stats;

public record GetBalanceByDayRequest : IRequest<IEnumerable<BalanceByDay>>
{
    public GetBalanceByDayRequest(Guid? exerciceId = null)
    {
        ExerciceId = exerciceId;
    }
    public Guid? ExerciceId { get; set; }
}
