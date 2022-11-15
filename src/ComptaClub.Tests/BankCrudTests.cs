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
    public class BankCrudTests
    {
        [TestMethod]
        public async Task Bank_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var bank = await mediator.Send(new Requests.GetBankByCodeRequest("fake"));
            bank.Should().BeNull();

            var name = $"Bank{Guid.NewGuid()}";
            bank = await mediator.Send(new CreateBankRequest(name, "test"));

            var saveResult = await mediator.Send(new SaveEntityRequest<Models.Bank>(bank));
            saveResult.HasError.Should().BeFalse();

            bank = await mediator.Send(new GetBankByCodeRequest(name));
            bank.Should().NotBeNull();

            bank.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntityRequest<Models.Bank>(bank));
            saveResult.HasError.Should().BeFalse();  
        }

    }
}