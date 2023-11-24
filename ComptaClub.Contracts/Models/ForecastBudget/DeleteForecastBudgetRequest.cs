using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.ForecastBudget;
public record DeleteForecastBudgetRequest(Guid ForecastBudgetId)
    : IRequest<CommandResult>;
