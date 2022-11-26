using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;

using FluentAssertions;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
    [TestClass]

    public class AccountCrudTests
    {
        [TestMethod]
        public async Task Account_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var account = await mediator.Send(new Requests.GetAccountByCodeRequest("fake"));
            account.Should().BeNull();   

            var code = $"Test{Guid.NewGuid()}";
            account = await mediator.Send(new Requests.CreateAccountRequest(code, $"{Guid.NewGuid()}", Models.AccountDirection.Credit));

            var saveResult = await mediator.Send(new Requests.SaveEntityRequest<Models.Account>(account));
            saveResult.HasError.Should().BeFalse();

            var label = account.Label = account.Label + $"{Guid.NewGuid()}";

            saveResult = await mediator.Send(new Requests.SaveEntityRequest<Models.Account>(account));
            saveResult.HasError.Should().BeFalse();

            account = await mediator.Send(new Requests.GetAccountByCodeRequest(code));
            account.Should().NotBeNull();

            account.Label.Should().Be(label);
        }

    }
}
