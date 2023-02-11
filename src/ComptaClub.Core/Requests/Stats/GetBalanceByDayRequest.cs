namespace ComptaClub.Requests.Stats;

public record GetBalanceByDayRequest : IRequest<IEnumerable<BalanceByDay>>
{
    public GetBalanceByDayRequest(Guid? exerciceId = null)
    {
        ExerciceId = exerciceId;
    }
    public Guid? ExerciceId { get; set; }
}
