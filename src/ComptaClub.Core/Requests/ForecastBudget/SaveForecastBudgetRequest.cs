using ComptaClub.Results;

namespace ComptaClub.Requests.ForecastBudget;
public record SaveForecastBudgetRequest(ForecastBudgetData ForecastBudget) 
	: IRequest<PersistResult>;