using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class CreateForecastBudgetTests
{
	[TestInitialize]
	public async Task Initialize()
	{
		var app = await TestHelper.CreateWebApplication();
		await TestHelper.CleanupDatabase(app.Services);
	}

	[TestMethod]
	public async Task Create_Forecast_Budget_From_Income_Statement()
	{
		var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

		var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
		var leafPlan = plan.GetLeafList();
		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var licenceAccount = leafPlan.Single(i => i.Code == "756001");

		await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);
		await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);

		var materialAccount = leafPlan.Single(i => i.Code == "605001");
		var gaz = leafPlan.Single(i => i.Code == "606101");
		await mediator.CreateAndSaveRandomDebitEntry(exercice, user, bank, gaz.Id, DateTime.Now);
		await mediator.CreateAndSaveRandomDebitEntry(exercice, user, bank, materialAccount.Id, DateTime.Now);

		var closeExerciceResult = await mediator.Send(new CloseExerciceRequest(exercice.Id));
		closeExerciceResult.HasError.Should().BeFalse();

		var incomeStatementResult = await mediator.Send(new CreateAndSaveIncomeStatementRequest(exercice.Id));
		incomeStatementResult.Should().NotBeNull();

		var incomeStatement = await mediator.GetIncomeStatementDataById(incomeStatementResult.Id);
		var incomeStatementCreditTotal = incomeStatement!.CreditTotal;
		var incomeStatementDebitTotal = incomeStatement.DebitTotal;


		var saveForecastBudgetResult = await mediator.Send(new CreateAndSaveForecastBudgetRequest(incomeStatementResult.Id,
			"Test",
			"Test"));
		saveForecastBudgetResult.Should().NotBeNull();

		var forecastBudget = await mediator.GetForecastBudgetById(saveForecastBudgetResult.Id);
		forecastBudget.Should().NotBeNull();

		forecastBudget!.ItemList = await mediator.GetForecastBudgetItemList(saveForecastBudgetResult.Id);

		var firstItem = forecastBudget!.ItemList[0];
		firstItem.Amount = 100;

		var name = forecastBudget!.Name = TestHelper.GetRandomName();
		var description = forecastBudget.Description = TestHelper.GetRandomName();

		forecastBudget.ItemList.Levelize();
		forecastBudget.ItemList.Hierarchize();

		var saveResult = await mediator.Send(new SaveForecastBudgetRequest(forecastBudget));
		saveResult.Should().NotBeNull();

		forecastBudget = await mediator.GetForecastBudgetById(saveForecastBudgetResult.Id);
		forecastBudget!.Name.Should().Be(name);
		forecastBudget.Description.Should().Be(description);

		forecastBudget.IncomeStatementCreditTotal.Should().Be(incomeStatementCreditTotal);
		forecastBudget.IncomeStatementDebitTotal.Should().Be(incomeStatementDebitTotal);

		forecastBudget.ItemList = await mediator.GetForecastBudgetItemList(saveForecastBudgetResult.Id);

		firstItem = forecastBudget.ItemList[0];
		firstItem.Amount.Should().Be(100);

		var deleteResult = await mediator.Send(new DeleteForecastBudgetRequest(saveForecastBudgetResult.Id));
		deleteResult.Should().NotBeNull();

		forecastBudget = await mediator.GetForecastBudgetById(saveForecastBudgetResult.Id);
		forecastBudget.Should().BeNull();

		var forecastBudgetItems = await mediator.GetForecastBudgetItemList(saveForecastBudgetResult.Id);
		forecastBudgetItems.Count().Should().Be(0);
	}
}
