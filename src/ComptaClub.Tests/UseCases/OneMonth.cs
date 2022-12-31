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
public class OneMonth
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
    public async Task Write_One_Month_Entries()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

        var exerciceCode = $"Exercice {Guid.NewGuid()}";
        var exercice = await mediator.Send(new GetExerciceByFilterRequest(f => f.Code == exerciceCode));
        var startAmount = Convert.ToInt64(4041.06 * 1000000);
        if (exercice == null)
        {
            exercice = await mediator.Send(new Requests.CreateExerciceRequest(exerciceCode, 
                $"{Guid.NewGuid()}", 
                new DateTime(2021,1,1).FirstDateOfCurrentYear(),
                new DateTime(2021, 1, 1).LastDateOfCurrentYear(), 
                startAmount, 
                true));
            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
            saveResult.HasError.Should().BeFalse();
        }

        var plan = await TestHelper.GetOrCreatePlan(mediator)!;
        var bank = await TestHelper.GetOrCreateBank("MonthBank", mediator);

        var licenceAccount = await mediator.Send(new GetAccountByFilterRequest(i => i.Code == "756001"))!;
        var bankFeeAccount = await mediator.Send(new GetAccountByFilterRequest(i => i.Code == "627001"))!;

        var entry = await mediator.Send(new CreateEntryRequest());
        entry.CreationDate = entry.ValueDate = new DateTime(2021, 2, 1).ToDayId();
        entry.Label = "B70027L NICOLAS GIRARD 2/2\r\nB70027L Adhesion Nicolas Girard 2/2";
        entry.PartNumber = "VIR M NICOLAS GIRARD";
        entry.BankId = bank.Id;
        entry.AccountId = licenceAccount.Id;
        entry.ExerciceId = exercice.Id;
        entry.Amount = 70 * 1000000;
        entry.AccountDirection = licenceAccount.Direction;
        entry.PaymentType = Datas.PaymentType.Transfer;

        var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveEntryResult.HasError.Should().BeFalse();

        entry = await mediator.Send(new CreateEntryRequest());
        entry.CreationDate = entry.ValueDate = new DateTime(2021, 2, 3).ToDayId();
        entry.Label = "F FRAIS UTIL DOMIWEB DECE.19";
        entry.PartNumber = "NE05093530\r\n";
        entry.BankId = bank.Id;
        entry.AccountId = bankFeeAccount.Id;
        entry.ExerciceId = exercice.Id;
        entry.Amount = Convert.ToInt64(2.45 * 1000000);
        entry.AccountDirection = bankFeeAccount.Direction;
        entry.PaymentType = Datas.PaymentType.Transfer;

        saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
        saveEntryResult.HasError.Should().BeFalse();

    }
}
