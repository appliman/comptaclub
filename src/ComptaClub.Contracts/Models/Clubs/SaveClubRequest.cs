using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Clubs;

public record SaveClubRequest(ClubData Club)
    : IRequest<PersistResult>;