using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

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

            var user = await mediator.Send(new Requests.GetUserByFilterRequest(i => i.Email = "fake"));
            user.Should().BeNull();

            var userEmail = $"{Guid.NewGuid()}@email.com";
            var userName = $"{Guid.NewGuid()}";

            user = await mediator.Send(new Requests.CreateUserRequest(userName, userEmail));

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.UserData>(user));
            saveResult.HasError.Should().BeFalse();

            user = await mediator.Send(new Requests.GetUserByFilterRequest(i => i.Email = userEmail));
            user.Should().NotBeNull();

            user!.Name.Should().Be(userName);

            userName = user.Name = $"{Guid.NewGuid()}";
            saveResult = await mediator.Send(new SaveEntityRequest<Datas.UserData>(user));
            saveResult.HasError.Should().BeFalse();

            user.Name.Should().Be(userName);

            var disableResult = await mediator.Send(new Requests.DisableUserRequest(user.Id));
            disableResult.HasError.Should().BeFalse();

            user = await mediator.Send(new Requests.GetUserByFilterRequest(f => {
                f.Email = userEmail;
                }));
            user.Should().BeNull();

            user = await mediator.Send(new Requests.GetUserByFilterRequest(f => {
                f.Email = userEmail;
                f.Options.DeletedState = Models.DeletedState.Both;
            }));
            user.Should().NotBeNull();

        }

    }
}
