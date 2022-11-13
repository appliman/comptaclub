using ComptaClub.Datas;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
    [TestClass]
    public class ExerciceCrudTests
    {
        [TestMethod]
        public async Task Exercice_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var exercice = await mediator.Send(new Requests.GetExerciceByCode("fake"));
            exercice.Should().BeNull();

            var name = $"Ex{Guid.NewGuid()}";
            exercice = await mediator.Send(new CreateExercice(name, 
                "test", 
                new DateTime(DateTime.Now.Year, 1, 1, 0,0,0).ToUniversalTime(),
                new DateTime(DateTime.Now.Year, 1, 1).AddYears(1).AddDays(-1).ToUniversalTime(), 
                100 * 1000000));

            var saveResult = await mediator.Send(new SaveEntity<Models.Exercice>(exercice));
            saveResult.HasError.Should().BeFalse();

            exercice = await mediator.Send(new GetExerciceByCode(name));
            exercice.Should().NotBeNull();

            exercice.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntity<Models.Exercice>(exercice));
            saveResult.HasError.Should().BeFalse();
        }
    }
}
