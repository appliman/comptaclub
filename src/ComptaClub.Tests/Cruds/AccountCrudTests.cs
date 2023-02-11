using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class AccountCrudTests
    {
        [TestInitialize]
        public async Task Initialize()
        {
            var app = await TestHelper.CreateWebApplication();
            await TestHelper.CleanupDatabase(app.Services);
        }

        [TestMethod]
        public async Task Account_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var account = await mediator.Send(new Requests.Accounts.GetAccountByFilterRequest(i => i.Code = "fake"));
            account.Should().BeNull();

            var code = $"Test{Guid.NewGuid()}";
            account = await mediator.Send(new Requests.Accounts.CreateAccountRequest(code, $"{Guid.NewGuid()}", Datas.AccountDirection.Credit));

            var saveResult = await mediator.Send(new Requests.SaveEntityRequest<Datas.AccountData>(account));
            saveResult.HasError.Should().BeFalse();

            var label = account.Label = account.Label + $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new Requests.SaveEntityRequest<Datas.AccountData>(account));
            saveResult.HasError.Should().BeFalse();

            account = await mediator.Send(new Requests.Accounts.GetAccountByFilterRequest(i => i.Code = code));
            account.Should().NotBeNull();

            account!.Label.Should().Be(label);

            var commandResult = await mediator.Send(new Requests.Accounts.DeleteAccountRequest(account.Id));
            commandResult.HasError.Should().BeFalse();
            commandResult.ChangeCount.Should().Be(1);

            account = await mediator.Send(new Requests.Accounts.GetAccountByFilterRequest(i => i.Code = code));
            account.Should().BeNull();
        }

    }
}
