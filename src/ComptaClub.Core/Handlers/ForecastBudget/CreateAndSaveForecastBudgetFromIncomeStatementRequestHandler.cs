using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Contracts.Results;
using ComptaClub.Enums;

namespace ComptaClub.Handlers.ForecastBudget;
internal class CreateAndSaveForecastBudgetFromIncomeStatementRequestHandler : IRequestHandler<CreateAndSaveForecastBudgetFromIncomeStatementRequest, PersistResult>
{
	private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
	private readonly IMediator _mediator;
	private readonly ILogger<CreateAndSaveForecastBudgetFromIncomeStatementRequestHandler> _logger;

	public CreateAndSaveForecastBudgetFromIncomeStatementRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory,
		IMediator mediator,
		ILogger<CreateAndSaveForecastBudgetFromIncomeStatementRequestHandler> logger)
	{
		_dbContextFactory = dbContextFactory;
		_mediator = mediator;
		_logger = logger;
	}

	public async Task<PersistResult> Handle(CreateAndSaveForecastBudgetFromIncomeStatementRequest request, CancellationToken cancellationToken)
	{
		var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var persistResult = new PersistResult();

		var incomeStatement = await _mediator.GetIncomeStatementDataById(request.IncomeStatementId);
		if (incomeStatement == null)
		{
			return PersistResult.CreateInvalidResult("Le compte de résultat n'existe pas");
		}

		var itemFilter = new IncomeStatementItemListFilter
		{
			IncomeStatementId = request.IncomeStatementId,
			PageSize = int.MaxValue
		};
		var incomeStatementItemList = await _mediator.Send(new GetPagedEntityListRequest<IncomeStatementItemListFilter, IncomeStatementItemData>(itemFilter));

		var forecastBudget = new ForecastBudgetData
		{
			Id = Guid.NewGuid(),
			IncomeStatementId = request.IncomeStatementId,
			Name = request.Name,
			Description = request.Description,
			CreationDate = DateTime.Now.ToDayId()
		};

		db.ForecastBudgets.Add(forecastBudget);

		var accountList = await _mediator.GetAllAccounts();
		accountList.Levelize();

		foreach (var account in accountList.OrderBy(i => i.Level))
		{
			var forecastBudgetItem = new Datas.ForecastBudgetItemData()
			{
				Id = Guid.NewGuid(),
				AccountId = account.Id,
				AccountCode = account.Code,
				ForecastBudgetId = forecastBudget.Id,
				AccountLabel = account.Label,
				Direction = account.Direction,
			};

			var parent = forecastBudget.ItemList.Find(i => i.AccountId == account.ParentAccountId);
			if (parent is not null)
			{
				forecastBudgetItem.ParentForecastBudgetItemId = parent.Id;
			}

			var isi = incomeStatementItemList.List.SingleOrDefault(i => i.AccountId == account.Id);
			if (isi is not null)
			{
				forecastBudgetItem.IncomeStatementAmount = isi.Amount;
				forecastBudgetItem.Amount = isi.Amount;
			}

			forecastBudget.ItemList.Add(forecastBudgetItem);
			db.ForecastBudgetItems.Add(forecastBudgetItem);
		}

		foreach (var item in forecastBudget.ItemList)
		{
			item.Level = item.ParentForecastBudgetItemId is null ? 0 : -1;
		}

		forecastBudget.ItemList.Levelize();
		forecastBudget.ItemList.Hierarchize();

		forecastBudget.DebitTotal = forecastBudget.ItemList.Where(i => i.Direction == AccountDirection.Debit && !i.Children.Any()).Sum(i => i.Amount);
		forecastBudget.CreditTotal = forecastBudget.ItemList.Where(i => i.Direction == AccountDirection.Credit && !i.Children.Any()).Sum(i => i.Amount);

		forecastBudget.IncomeStatementDebitTotal = incomeStatement.DebitTotal;
		forecastBudget.IncomeStatementCreditTotal = incomeStatement.CreditTotal;

		using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			persistResult.ChangeCount = await db.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			persistResult.Id = forecastBudget.Id;
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			_logger.LogError(ex, "Error while saving forecast budget");
			persistResult.HasError = true;
			persistResult.AddErrorBrokenRule("All", "Error while saving forecast budget");
		}

		return persistResult;
	}
}
