namespace ComptaClub.Requests.Banks;

public record GetAllBanksRequest : IRequest<List<BankData>>
{
}
