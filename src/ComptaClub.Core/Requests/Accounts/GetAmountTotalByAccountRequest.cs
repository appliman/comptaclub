namespace ComptaClub.Requests.Accounts;

public record GetAmountTotalByAccountRequest(Guid? ExerciceId = null) 
    : IRequest<IEnumerable<AmountTotalByAccount>>;
