using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
    [TestClass]

    public class UserCrud
    {
        [TestMethod]
        public async Task User_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var userEmail = $"{Guid.NewGuid()}@email.com";

            var account = await mediator.Send(new Requests.GetUserByFilterRequest(i => i.Email == "fake"));
            account.Should().BeNull();

        }

    }
}
