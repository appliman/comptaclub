using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Datas;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class CreateIncomeStatementTests
{
	[TestInitialize]
	public async Task Initialize()
	{
		var app = await TestHelper.CreateWebApplication();
		await TestHelper.CleanupDatabase(app.Services);
	}

	[TestMethod]
	public async Task Create_Income_Statement_From_Closed_Exercice()
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

		var filter = new IncomeStatementListFilter();
		filter.IdList.Add(incomeStatementResult.Id);

		// Recupération du compte de résultat
		var incomeStatementList = await mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, IncomeStatementData>(filter));
		incomeStatementList.Should().NotBeNull();

		var incomeStatement = incomeStatementList.List.Single();

		var incomeStatementItemList = await mediator.Send(new GetPagedEntityListRequest<IncomeStatementItemListFilter, IncomeStatementItemData>(i => i.IncomeStatementId = incomeStatement.Id));
		incomeStatementItemList.Should().NotBeNull();
	}
}
