using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.ForecastBudget;
public record CreateAndSaveForecastBudgetRequest(string Name,
    string Description)
    : IRequest<PersistResult>;