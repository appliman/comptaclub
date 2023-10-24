using ComptaClub.Results;

namespace ComptaClub.Requests.Clubs;

public record SaveClubRequest(Datas.ClubData Club) 
	: IRequest<PersistResult>;