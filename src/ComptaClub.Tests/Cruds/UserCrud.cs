using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Users;

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
			var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

			var user = await mediator.Send(new GetUserByFilterRequest(i => i.Email = "fake"));
			user.Should().BeNull();

			var userEmail = $"{Guid.NewGuid()}@email.com";
			var userName = $"{Guid.NewGuid()}";

			user = await mediator.Send(new CreateUserRequest(userName, userEmail));

			var saveResult = await mediator.Send(new SaveEntityRequest<Datas.UserData>(user));
			saveResult.HasError.Should().BeFalse();

			user = await mediator.Send(new GetUserByFilterRequest(i => i.Email = userEmail));
			user.Should().NotBeNull();

			user!.Name.Should().Be(userName);

			userName = user.Name = $"{Guid.NewGuid()}";
			saveResult = await mediator.Send(new SaveEntityRequest<Datas.UserData>(user));
			saveResult.HasError.Should().BeFalse();

			user.Name.Should().Be(userName);

			var disableResult = await mediator.Send(new DisableUserRequest(user.Id));
			disableResult.HasError.Should().BeFalse();

			user = await mediator.Send(new GetUserByFilterRequest(f =>
			{
				f.Email = userEmail;
			}));
			user.Should().BeNull();

			user = await mediator.Send(new GetUserByFilterRequest(f =>
			{
				f.Email = userEmail;
				f.Options.DeletedState = DeletedState.Both;
			}));
			user.Should().NotBeNull();

		}

	}
}
