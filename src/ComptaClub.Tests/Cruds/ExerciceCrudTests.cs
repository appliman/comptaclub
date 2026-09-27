using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
	[TestClass]
	public class ExerciceCrudTests
	{
		[TestMethod]
		public async Task Exercice_Crud()
		{
			await using var app = await TestHelper.CreateWebApplication();
			var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

			var exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == "fake"));
			exercice.Should().BeNull();

			var name = $"Ex{Guid.NewGuid()}";
			exercice = await mediator.Send(new CreateExerciceRequest(name,
				"test",
				100 * 1000000));

			exercice.StartDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Local).ToDayId();
			exercice.EndDate = new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59, DateTimeKind.Local).AddYears(1).AddDays(-1).ToDayId();


			var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
			saveResult.HasError.Should().BeFalse();

			var exerciceId = saveResult.Id;

			exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == name));
			exercice.Should().NotBeNull();

			exercice!.Label = $"{Guid.NewGuid()}";

			saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
			saveResult.HasError.Should().BeFalse();

			var deleteResult = await mediator.Send(new DeleteExerciceRequest(exercice.Id));
			deleteResult.HasError.Should().BeFalse();
			deleteResult.ChangeCount.Should().Be(1);

			exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == name));
			exercice.Should().BeNull();

			deleteResult = await mediator.Send(new DeleteExerciceRequest(exerciceId));
			deleteResult.HasWarning.Should().BeTrue();
		}

		[TestMethod]
		public async Task Change_Active_Exercice()
		{
			await using var app = await TestHelper.CreateWebApplication();
			var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

			var exercice1 = await mediator.Send(new CreateExerciceRequest($"Ex{Guid.NewGuid()}",
				"Exercice1",
				100 * 1000000));

			exercice1.StartDate = new DateTime(DateTime.Now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Local).ToDayId();
            exercice1.EndDate = new DateTime(DateTime.Now.Year - 1, 1, 1, 23, 59, 59, DateTimeKind.Local).AddYears(1).AddDays(-1).ToDayId();

			var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice1));
			saveResult.HasError.Should().BeFalse();

			// Seul exercice il doit etre actif
			exercice1.Active.Should().BeTrue();

			var exercice2 = await mediator.Send(new CreateExerciceRequest($"Ex{Guid.NewGuid()}",
				"Exercice2",
				100 * 1000000));

			exercice2.StartDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Local).ToDayId();
			exercice2.EndDate = new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59, DateTimeKind.Local).AddYears(1).AddDays(-1).ToDayId();

			saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice2));
			saveResult.HasError.Should().BeFalse();

			exercice2.Active.Should().BeFalse();

			// On tente de passer cet exercice en actif
			exercice2.Active = true;

			saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice2));
			saveResult.HasError.Should().BeTrue();

			exercice2.Active = false;
			saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice2));
			saveResult.HasError.Should().BeFalse();

			var changeActiveExerciceResult = await mediator.Send(new ChangeActiveExerciceRequest(true, exercice2.Id));
			changeActiveExerciceResult.HasError.Should().BeFalse();

			var activeExercice = await mediator.Send(new GetActiveExerciceRequest());
			activeExercice.Should().NotBeNull();

			activeExercice!.Id.Should().Be(exercice2.Id);

			exercice1 = await mediator.Send(new GetExerciceByFilterRequest(f => f.Id == exercice1.Id));
			exercice1.Should().NotBeNull();
			exercice1!.Active.Should().BeFalse();
		}
	}
}
