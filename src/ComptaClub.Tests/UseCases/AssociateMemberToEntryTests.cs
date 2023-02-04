using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Extensions;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class AssociateMemberToEntryTests
{
    [TestInitialize]
    public async Task Initialize()
    {
        var app = await TestHelper.CreateWebApplication();
        await TestHelper.CleanupDatabase(app.Services);
    }

    [TestMethod]
    public async Task Associate_Entry()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

        var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
        var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
        var plan = await mediator.GetOrCreatePlan();
        var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");
        var leafPlan = plan.GetLeafList();

        var licenceAccount = leafPlan.Single(i => i.Code == "756001");
        var bankFeeAccount = leafPlan.Single(i => i.Code == "61/62");

        var entry = await mediator.Send(new CreateEntryRequest());
        entry.CreationDate = entry.ValueDate = new DateTime(DateTime.Now.Year, 2, 1).ToDayId();
        entry.Label = "B70027L NICOLAS GIRARD 2/2\r\nB70027L Adhesion Nicolas Girard 2/2";
        entry.PartNumber = "VIR M NICOLAS GIRARD";
        entry.BankId = bank.Id;
        entry.AccountId = licenceAccount!.Id;
        entry.ExerciceId = exercice.Id;
        entry.Amount = 70 * 1000000;
        entry.AccountDirection = licenceAccount.Direction;
        entry.PaymentType = Datas.PaymentType.Transfer;
        entry.UserCreatorId = user.Id;

        var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveEntryResult.HasError.Should().BeFalse();

        var member = await mediator.Send(new Requests.CreateMemberRequest());
        member.Name = $"{Guid.NewGuid()}";
        member.Email = $"{Guid.NewGuid()}@email.com";
        member.LicenseNumber = $"{Guid.NewGuid()}";

        var saveResult = await mediator.Send(new SaveEntityRequest<Datas.MemberData>(member));
        saveResult.HasError.Should().BeFalse();

        var associatedMemberList = await mediator.Send(new GetMemberListByEntryRequest(entry.Id));
        associatedMemberList.Any().Should().BeFalse();

        var assocResult = await mediator.Send(new LinkMemberToEntryRequest(entry.Id, member.Id));
        assocResult.HasError.Should().BeFalse();

        associatedMemberList = await mediator.Send(new GetMemberListByEntryRequest(entry.Id));
        associatedMemberList.Any().Should().BeTrue();

        var balanceList = await mediator.Send(new GetBalanceByMemberListRequest(f => f.PageSize = int.MaxValue, exercice.Id));
        balanceList.Any().Should().BeTrue();

        var balance = balanceList.Single();
        balance.Balance.Should().Be(entry.Amount);
    }
}
