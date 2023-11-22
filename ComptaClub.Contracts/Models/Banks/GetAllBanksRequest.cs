namespace ComptaClub.Contracts.Models.Banks;

public record GetAllBanksRequest : IRequest<List<BankData>>
{
}
