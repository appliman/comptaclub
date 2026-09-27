using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class AssociateMemberToEntryTests
{
	[TestMethod]
	public async Task Associate_Entry()
	{
		await using var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

		var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");
		var leafPlan = plan.GetLeafList();

		var licenceAccount = leafPlan.Single(i => i.Code == "756001");

		var entry = await mediator.Send(new CreateEntryRequest());
		entry.CreationDate = entry.ValueDate = new DateTime(DateTime.Now.Year, 2, 1).ToDayId();
		entry.Label = "B70027L NICOLAS GIRARD 2/2\r\nB70027L Adhesion Nicolas Girard 2/2";
		entry.PartNumber = "VIR M NICOLAS GIRARD";
		entry.BankId = bank.Id;
		entry.AccountId = licenceAccount!.Id;
		entry.ExerciceId = exercice.Id;
		entry.Amount = 70 * 1000000;
		entry.AccountDirection = licenceAccount.Direction;
		entry.PaymentType = Enums.PaymentType.Transfer;
		entry.UserCreatorId = user.Id;

		var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
		saveEntryResult.HasError.Should().BeFalse();

		var member1 = await mediator.Send(new CreateMemberRequest());
		member1.Name = $"{Guid.NewGuid()}";
		member1.Email = $"{Guid.NewGuid()}@email.com";
		member1.LicenseNumber = $"{Guid.NewGuid()}";

		var saveResult = await mediator.Send(new SaveEntityRequest<Datas.MemberData>(member1));
		saveResult.HasError.Should().BeFalse();

		var member2 = await mediator.Send(new CreateMemberRequest());
		member2.Name = $"{Guid.NewGuid()}";
		member2.Email = $"{Guid.NewGuid()}@email.com";
		member2.LicenseNumber = $"{Guid.NewGuid()}";

		saveResult = await mediator.Send(new SaveEntityRequest<Datas.MemberData>(member2));
		saveResult.HasError.Should().BeFalse();

		var associatedMemberList = await mediator.Send(new GetAssociatedMemberListByEntryRequest(entry.Id));
		associatedMemberList.Any().Should().BeFalse();

		var assocResult1 = await mediator.Send(new LinkMemberToEntryRequest(entry.Id, member1.Id, 30 * 1000000));
		assocResult1.HasError.Should().BeFalse();

		var assocResult2 = await mediator.Send(new LinkMemberToEntryRequest(entry.Id, member2.Id, 40 * 1000000));
		assocResult2.HasError.Should().BeFalse();

		associatedMemberList = await mediator.Send(new GetAssociatedMemberListByEntryRequest(entry.Id));
		associatedMemberList.Any().Should().BeTrue();

		var balanceList = await mediator.Send(new GetBalanceByMemberListRequest(f =>
		{
			f.PageSize = int.MaxValue;
			f.GetById(member1.Id);
		}, exercice.Id));

		balanceList.Any().Should().BeTrue();

		var balance = balanceList.Single();
		balance.Balance.Should().Be(30 * 1000000);

		var unlinkResult = await mediator.Send(new UnlinkMemberToEntryRequest(assocResult2.Id));
		unlinkResult.HasError.Should().BeFalse();
		unlinkResult.ChangeCount.Should().Be(1);
	}
}
