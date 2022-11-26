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
public class FirstEntry
{
	/// <summary>
	/// Ecriture de la première entrée
	/// 
	/// 1 - Creer un premier exercice sur l'année en cours avec un solde de 100€
	/// 2 - Ajout d'un plan de compta
	/// 3 - Creation de la banque
	/// 4 - Ajouter une entrée de 50
	/// 
	/// Verifier que le nouveau solde est de 50€
	/// </summary>
	/// <returns></returns>
	[TestMethod]
	public async Task Write_First_Entry()
	{
		var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

		var exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == "Exercice 2022"));
		if (exercice == null)
		{
			exercice = await mediator.Send(new Requests.CreateExerciceRequest("Exercice 2022", "Exercice 2022", DateTime.Now.FirstDateOfCurrentYear(), DateTime.Now.LastDateOfCurrentYear(), 100 * 1000000, true));
			var saveResult = await mediator.Send(new SaveEntityRequest<Models.Exercice>(exercice));
			saveResult.HasError.Should().BeFalse();
		}

		var planFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "InitialAccountingPlan.json");
		var planFileContent = System.IO.File.ReadAllText(planFile);

		var plan = System.Text.Json.JsonSerializer.Deserialize<List<Models.Account>>(planFileContent, ComptaClub.JsonSerializer.Options);	

		var importResult = await mediator.Send(new ImportAccountingPlanRequest(plan!));

		var bank = await mediator.Send(new GetBankByCodeRequest("MyBank"));
		if (bank == null)
		{
			bank = await mediator.Send(new CreateBankRequest("MyBank", "My Bank"));
			var saveResult = await mediator.Send(new SaveEntityRequest<Models.Bank>(bank));	
			saveResult.HasError.Should().BeFalse();
		}

		var account = await mediator.Send(new GetAccountByCodeRequest("605001"));

		var entry = await mediator.Send(new CreateEntryRequest());
		entry.PartNumber = $"{Guid.NewGuid()}";
		entry.Label = "My first entry";
		entry.BankId = bank.Id;
		entry.AccountId = account.Id;
		entry.ExerciceId = exercice.Id;
		entry.Amount = 40 * 1000000;
		entry.AccountDirection = account.Direction;
		entry.PaymentType = Models.PaymentType.CreditCard;

		var balance = await mediator.Send(new Requests.ApplyBalanceForEntryRequest(entry));
		balance.Should().NotBeNull();
		balance.Amount.Should().Be(60 * 1000000);

		var saveEntryResult = await mediator.Send(new SaveEntryRequest(entry));
		saveEntryResult.HasError.Should().BeFalse();
	}
}
