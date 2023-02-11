using ComptaClub.Datas;
using ComptaClub.Extensions;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
    [TestClass]
    public class ExerciceCrudTests
    {
        [TestMethod]
        public async Task Exercice_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var exercice = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Code == "fake"));
            exercice.Should().BeNull();

            var name = $"Ex{Guid.NewGuid()}";
            exercice = await mediator.Send(new Requests.Exercices.CreateExerciceRequest(name,
                "test",
                new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0).ToDayId(),
                new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59).AddYears(1).AddDays(-1).ToDayId(),
                100 * 1000000));

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
            saveResult.HasError.Should().BeFalse();

            var exerciceId = saveResult.Id;

            exercice = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Code == name));
            exercice.Should().NotBeNull();

            exercice!.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
            saveResult.HasError.Should().BeFalse();

            var deleteResult = await mediator.Send(new Requests.Exercices.DeleteExerciceRequest(exercice.Id));
            deleteResult.HasError.Should().BeFalse();
            deleteResult.ChangeCount.Should().Be(1);

			exercice = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Code == name));
			exercice.Should().BeNull();

			deleteResult = await mediator.Send(new Requests.Exercices.DeleteExerciceRequest(exerciceId));
			deleteResult.HasWarning.Should().BeTrue();
		}

		[TestMethod]
		public async Task Change_Active_Exercice()
        {
			var app = await TestHelper.CreateWebApplication();
			var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

			var exercice1 = await mediator.Send(new Requests.Exercices.CreateExerciceRequest($"Ex{Guid.NewGuid()}",
				"Exercice1",
				new DateTime(DateTime.Now.Year - 1, 1, 1, 0, 0, 0).ToDayId(),
				new DateTime(DateTime.Now.Year - 1, 1, 1, 23, 59, 59).AddYears(1).AddDays(-1).ToDayId(),
				100 * 1000000));

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice1));
            saveResult.HasError.Should().BeFalse();

            // Seul exercice il doit etre actif
            exercice1.Active.Should().BeTrue();

			var exercice2 = await mediator.Send(new Requests.Exercices.CreateExerciceRequest($"Ex{Guid.NewGuid()}",
				"Exercice2",
				new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0).ToDayId(),
				new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59).AddYears(1).AddDays(-1).ToDayId(),
				100 * 1000000));

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

            var changeActiveExerciceResult = await mediator.Send(new Requests.Exercices.ChangeActiveExerciceRequest(true, exercice2.Id));
            changeActiveExerciceResult.HasError.Should().BeFalse();

            var activeExercice = await mediator.Send(new Requests.Exercices.GetActiveExerciceRequest());
            activeExercice.Should().NotBeNull();

			activeExercice!.Id.Should().Be(exercice2.Id);

            exercice1 = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Id == exercice1.Id));
            exercice1.Should().NotBeNull();
            exercice1!.Active.Should().BeFalse();
		}
	}
}
