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
public class FirstEntryTests
{
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
	public async Task Write_First_Entry()
	{
		var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var exercice = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Code == "Exercice 2022"));
		if (exercice == null)
		{
			exercice = await mediator.Send(new Requests.Exercices.CreateExerciceRequest("Exercice 2022", "Exercice 2022", DateTime.Now.FirstDateOfCurrentYear(), DateTime.Now.LastDateOfCurrentYear(), 100 * 1000000));
			var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
			saveResult.HasError.Should().BeFalse();
		}

		var planFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "InitialAccountingPlan.json");
		var planFileContent = System.IO.File.ReadAllText(planFile);

		var plan = System.Text.Json.JsonSerializer.Deserialize<List<Datas.AccountData>>(planFileContent, ComptaClub.JsonSerializer.Options);	

		var importResult = await mediator.Send(new Requests.Accounts.ImportAccountingPlanRequest(plan!));
		importResult.HasError.Should().BeFalse();

		var bank = await mediator.Send(new Requests.Banks.GetBankByFilterRequest(i => i.Code == "MyBank"));
		if (bank == null)
		{
			bank = await mediator.Send(new Requests.Banks.CreateBankRequest("MyBank", "My Bank"));
			var saveResult = await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));	
			saveResult.HasError.Should().BeFalse();
		}

		var account = await mediator.Send(new Requests.Accounts.GetAccountByFilterRequest(i => i.Code = "605001"));
		account.Should().NotBeNull();

		var entry = await mediator.Send(new Requests.Entries.CreateEntryRequest());
		entry.PartNumber = $"{Guid.NewGuid()}";
		entry.Label = "My first entry";
		entry.BankId = bank.Id;
		entry.AccountId = account!.Id;
		entry.ExerciceId = exercice.Id;
		entry.Amount = 40 * 1000000;
		entry.AccountDirection = account.Direction;
		entry.PaymentType = Enums.PaymentType.CreditCard;
		entry.UserCreatorId = user.Id;

		var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
		saveEntryResult.HasError.Should().BeFalse();

		exercice = await mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(i => i.Id == entry.ExerciceId))!;
		exercice!.BalanceAmount.Should().Be(60 * 1000000);
		exercice!.LastEntryId.Should().Be(entry.Id);
	}
}
