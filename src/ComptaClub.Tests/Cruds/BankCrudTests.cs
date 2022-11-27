using ComptaClub.Datas;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
    [TestClass]
    public class BankCrudTests
    {
        [TestMethod]
        public async Task Bank_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var bank = await mediator.Send(new GetBankByFilterRequest(i => i.Code == "fake"));
            bank.Should().BeNull();

            var name = $"Bank{Guid.NewGuid()}";
            bank = await mediator.Send(new CreateBankRequest(name, "test"));

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));
            saveResult.HasError.Should().BeFalse();

            bank = await mediator.Send(new GetBankByFilterRequest(i => i.Code == name));
            bank.Should().NotBeNull();

            bank!.Label = $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));
            saveResult.HasError.Should().BeFalse();
        }

    }
}