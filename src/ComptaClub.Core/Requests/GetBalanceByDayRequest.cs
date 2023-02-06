namespace ComptaClub.Requests;

public record GetBalanceByDayRequest : IRequest<IEnumerable<Models.BalanceByDay>>
{
	public GetBalanceByDayRequest(Guid? exerciceId = null)
	{
		this.ExerciceId = exerciceId;
	}
    public Guid? ExerciceId { get; set; }
}
