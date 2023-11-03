using ComptaClub.Extensions;
using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds;

[TestClass]
public class EntryCrudTests
{
    [TestInitialize]
    public async Task Initialize()
    {
        var app = await TestHelper.CreateWebApplication();
        await TestHelper.CleanupDatabase(app.Services);
    }

    [TestMethod]
    public async Task Entry_Crud()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

        var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
        var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
        var plan = await mediator.GetOrCreatePlan()!;
        var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");
        var leafList = plan.GetLeafList();

        var entry = await mediator.Send(new Requests.Entries.GetEntryByFilterRequest(i => i.GetById(Guid.NewGuid())));
        entry.Should().BeNull();

        entry = await mediator.Send(new Requests.Entries.CreateEntryRequest());
        entry.Should().NotBeNull();

        var saveResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveResult.HasError.Should().BeTrue();

        var firstAccount = leafList.First();
        entry.ExerciceId = exercice.Id;
        entry.BankId = bank.Id;
        entry.Amount = 10 * 1000000;
        entry.AccountId = firstAccount.Id;
        entry.UserCreatorId = user.Id;
        entry.AccountDirection = firstAccount.Direction;
        var partNumber = entry.PartNumber = $"{Guid.NewGuid()}";
        var label = entry.Label = $"{Guid.NewGuid()}";
        var paymentType = entry.PaymentType = Enums.PaymentType.Transfer;

        saveResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveResult.HasError.Should().BeFalse();

        var today = DateTime.Today.ToDayId();

        entry.ExerciceId.Should().Be(exercice.Id);
        entry.BankId.Should().Be(bank.Id);
        entry.Amount.Should().Be(10 * 1000000);
        entry.AccountId.Should().Be(leafList.First().Id);
        entry.PartNumber.Should().Be(partNumber);
        entry.Label.Should().Be(label);
        entry.CreationDate.Should().Be(today);
        entry.ValueDate.Should().Be(today);
        entry.DeletedDate.Should().BeNull();
        entry.AccountDirection.Should().Be(firstAccount.Direction);
        entry.ImportId.Should().BeNull();
        entry.PaymentType.Should().Be(paymentType);
        entry.UserCreatorId.Should().Be(user.Id);

        entry.Amount = 20 * 1000000;
        entry.ExtraInfos = "Test";

        saveResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveResult.HasError.Should().BeFalse();

        entry = await mediator.Send(new Requests.Entries.GetEntryByFilterRequest(i => i.GetById(entry.Id)));

        entry!.Amount.Should().Be(20 * 1000000);
        entry!.ExtraInfos.Should().Be("Test");

        var deleteResult = await mediator.Send(new Requests.Entries.DeleteEntryRequest(entry!.Id));
        deleteResult.HasError.Should().BeFalse();

        entry = await mediator.Send(new Requests.Entries.GetEntryByFilterRequest(i => i.GetById(entry.Id)));
        entry.Should().BeNull();
    }

}