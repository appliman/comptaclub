using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Extensions;
using ComptaClub.Requests;

using FluentAssertions;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ComptaClub.Tests.UseCases;

[TestClass]
public class OneMonthTests
{
	[TestInitialize]
	public async Task Initialize()
	{
		var app = await TestHelper.CreateWebApplication();
		await TestHelper.CleanupDatabase(app.Services);
	}

	/// <summary>
	/// Ecriture de la première entrée
	/// 
	/// 1 - Creer un premier exercice sur l'année en cours avec un solde de 100€
	/// 2 - Ajout d'un plan de compta
	/// 3 - Creation de la banque
	/// 4 - Ajouter une entrée de 40
	/// 
	/// Verifier que le nouveau solde est de 60€
	/// </summary>
	/// <returns></returns>
	[TestMethod]
    public async Task Write_One_Month_Entries()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

		var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
        var leafPlan = plan.GetLeafList();
        var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

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

        exercice = await mediator.Send(new GetExerciceByFilterRequest(i => i.Id == exercice.Id));
        exercice!.BalanceAmount.Should().Be(70 * 1000000);

        entry = await mediator.Send(new CreateEntryRequest());
        entry.CreationDate = entry.ValueDate = new DateTime(DateTime.Now.Year, 2, 3).ToDayId();
        entry.Label = "F FRAIS UTIL DOMIWEB DECE.19";
        entry.PartNumber = "NE05093530\r\n";
        entry.BankId = bank.Id;
        entry.AccountId = bankFeeAccount!.Id;
        entry.ExerciceId = exercice.Id;
        entry.Amount = Convert.ToInt64(2.45 * 1000000);
        entry.AccountDirection = bankFeeAccount.Direction;
        entry.PaymentType = Datas.PaymentType.Transfer;
        entry.UserCreatorId = user.Id;

        saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveEntryResult.HasError.Should().BeFalse();

        var balance = exercice.BalanceAmount - entry.Amount;
        exercice = await mediator.Send(new GetExerciceByFilterRequest(i => i.Id == exercice.Id));
        exercice!.BalanceAmount.Should().Be(balance);

        entry = await mediator.Send(new CreateEntryRequest());
        entry.CreationDate = entry.ValueDate = new DateTime(DateTime.Now.Year, 2, 3).ToDayId();
        entry.Label = "REM CHQ 3730053 0536 010 CHQ";
        entry.PartNumber = "REM CHQ 3730053 0536 010 CHQ";
        entry.BankId = bank.Id;
        entry.AccountId = licenceAccount!.Id;
        entry.ExerciceId = exercice.Id;
        entry.Amount = Convert.ToInt64(1209 * 1000000);
        entry.AccountDirection = licenceAccount.Direction;
        entry.PaymentType = Datas.PaymentType.Check;
        entry.UserCreatorId = user.Id;

        saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveEntryResult.HasError.Should().BeFalse();

        balance = exercice.BalanceAmount + entry.Amount;
        exercice = await mediator.Send(new GetExerciceByFilterRequest(i => i.Id == exercice.Id));
        exercice!.BalanceAmount.Should().Be(balance);
    }
}
