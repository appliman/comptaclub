using ComptaClub.Results;

namespace ComptaClub.Requests.ForecastBudget;
public record DeleteForecastBudgetRequest(Guid ForecastBudgetId)
    : IRequest<CommandResult>;
