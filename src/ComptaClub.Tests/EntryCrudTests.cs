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
    public class EntryCrudTests
    {
        [TestMethod]
        public async Task Entry_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var bank = await mediator.Send(new Requests.GetBankByCode("fake"));
            bank.Should().BeNull();

            var name = $"Bank{Guid.NewGuid()}";
            bank = await mediator.Send(new CreateBank(name, "test"));

            var saveResult = await mediator.Send(new SaveEntity<Models.Bank>(bank));
            saveResult.HasError.Should().BeFalse();

            bank = await mediator.Send(new GetBankByCode(name));
            bank.Should().NotBeNull();

            bank.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntity<Models.Bank>(bank));
            saveResult.HasError.Should().BeFalse();  
        }

    }
}