using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.ForecastBudget;
public record CreateAndSaveForecastBudgetRequest(Guid IncomeStatementId,
    string Name,
    string Description)
    : IRequest<PersistResult>;