using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.ForecastBudget;
public record SaveForecastBudgetRequest(ForecastBudgetData ForecastBudget)
    : IRequest<PersistResult>;