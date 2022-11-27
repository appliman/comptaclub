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

            var settings = app.Services.GetRequiredService<Configuration.ComptaClubSettings>();

            var exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == "fake"));
            exercice.Should().BeNull();

            var name = $"Ex{Guid.NewGuid()}";
            exercice = await mediator.Send(new CreateExerciceRequest(name,
                "test",
                new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0).ToDayId(),
                new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59).AddYears(1).AddDays(-1).ToDayId(),
                100 * 1000000));

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
            saveResult.HasError.Should().BeFalse();

            exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == name));
            exercice.Should().NotBeNull();

            exercice!.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
            saveResult.HasError.Should().BeFalse();
        }
    }
}
