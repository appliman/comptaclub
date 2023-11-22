namespace ComptaClub.Contracts.Models.Accounts;

public record GetAmountTotalByAccountRequest(Guid? ExerciceId = null)
    : IRequest<IEnumerable<AmountTotalByAccount>>;
