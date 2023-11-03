using ComptaClub.Results;

namespace ComptaClub.Requests.ForecastBudget;
public record CreateAndSaveForecastBudgetRequest(Guid IncomeStatementId, 
    string Name,
    string Description)
    : IRequest<PersistResult>;