using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class CreateNewExerciceAfterExistingTests
{
	[TestMethod]
	public async Task Create_Exercice_And_Create_New_Exercice()
	{
		await using var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

		var exerciceCode = $"{Guid.NewGuid()}";
		var exercice = await mediator.GetOrCreateExercice(exerciceCode);
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
		var leafPlan = plan.GetLeafList();
		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var licenceAccount = leafPlan.Single(i => i.Code == "756001");

		var entry1 = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);

		exercice = await mediator.Send(new GetExerciceByFilterRequest(i => i.Code == exerciceCode));
		var balance = exercice!.BalanceAmount;

		var newExercice = await mediator.Send(new CreateNextExerciceRequest(exercice.Id, "test", "test"));
		newExercice.Should().NotBeNull();

		newExercice!.BalanceAmount.Should().Be(balance);
	}
}
