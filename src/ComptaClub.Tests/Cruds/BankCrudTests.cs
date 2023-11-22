using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Banks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
	[TestClass]
	public class BankCrudTests
	{
		[TestInitialize]
		public async Task Initialize()
		{
			var app = await TestHelper.CreateWebApplication();
			await TestHelper.CleanupDatabase(app.Services);
		}

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