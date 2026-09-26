using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.ForecastBudget;
internal class DeleteForecastBudgetRequestHandler : IRequestHandler<DeleteForecastBudgetRequest, CommandResult>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public DeleteForecastBudgetRequestHandler(IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(DeleteForecastBudgetRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var forecastBudget = await db.ForecastBudgets.FirstOrDefaultAsync(i => i.Id == request.ForecastBudgetId, cancellationToken);
        if (forecastBudget is null)
        {
            return CommandResult.CreateInvalidResult("Ce compte de résultat n'existe pas");
        }

        db.ForecastBudgets.Remove(forecastBudget);

        var items = await db.ForecastBudgetItems.Where(i => i.ForecastBudgetId == request.ForecastBudgetId).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            db.ForecastBudgetItems.Remove(item);
        }

        using var transaction = db.Database.BeginTransaction();
        try
        {
            var changeCount = await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CommandResult()
            {
                ChangeCount = changeCount,
                HasError = false
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CommandResult.CreateInvalidResult(ex.Message);
        }
    }
}