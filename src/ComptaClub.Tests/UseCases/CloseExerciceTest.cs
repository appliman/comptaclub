using FluentAssertions;

using ComptaClub.Extensions;

using Microsoft.Extensions.DependencyInjection;


namespace ComptaClub.Tests.UseCases;

[TestClass]
public class CloseExerciceTest
{
	[TestInitialize]
	public async Task Initialize()
	{
		var app = await TestHelper.CreateWebApplication();
		await TestHelper.CleanupDatabase(app.Services);
	}

	/// <summary>
	/// Ajouter des entrées dans un exercice
	/// Le clore 
	/// Verifier que l'on ne peut plus ajouter d'entrées meme si celui-ci passe en cours
	/// </summary>
	/// <returns></returns>
	[TestMethod]
	public async Task Close_Exercice()
	{
		var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

		var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
		var leafPlan = plan.GetLeafList();
		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var licenceAccount = leafPlan.Single(i => i.Code == "756001");

		var entry1 = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);
		var entry2 = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);
		var entry3 = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);

		var closeExerciceResult = await mediator.Send(new Requests.Exercices.CloseExerciceRequest(exercice.Id));
		closeExerciceResult.HasError.Should().BeFalse();

		var entry4 = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);
		entry4.Should().BeNull();

	}

}
