using ComptaClub.Requests.ForecastBudget;
using ComptaClub.Results;

namespace ComptaClub.Handlers.ForecastBudget;
internal class SaveForecastBugetRequestHandler : IRequestHandler<SaveForecastBudgetRequest, PersistResult>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
	private readonly ILogger<SaveForecastBugetRequestHandler> _logger;

	public SaveForecastBugetRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory,
		ILogger<SaveForecastBugetRequestHandler> logger)
	{
		_dbContextFactory = dbContextFactory;
		_logger = logger;
	}

	public async Task<PersistResult> Handle(SaveForecastBudgetRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var persistResult = new PersistResult();

		db.ForecastBudgets.Attach(request.ForecastBudget);
		db.Entry(request.ForecastBudget).State = EntityState.Modified;

		AttachItemsToDb(request.ForecastBudget.ItemList, db);

		using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			persistResult.ChangeCount = await db.SaveChangesAsync(cancellationToken);
			transaction.Commit();
		}
		catch(Exception ex)
		{
			transaction.Rollback();
			_logger.LogError(ex, "Erreur lors de la sauvegarde du budget prévisionnel");
			persistResult.AddErrorBrokenRule("All", "Erreur lors de la sauvegarde du budget prévisionnel");
		}

		return persistResult;
	}

	void AttachItemsToDb(List<ForecastBudgetItemData> list, Datas.ComptaClubDbContext db)
	{
		foreach (var item in list)
		{
			db.ForecastBudgetItems.Attach(item);
			db.Entry(item).State = EntityState.Modified;
			AttachItemsToDb(item.Children, db);
		}
	}
}
