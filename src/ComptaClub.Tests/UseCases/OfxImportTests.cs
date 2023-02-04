using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Extensions;
using ComptaClub.Handlers;
using ComptaClub.Requests;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class OfxImportTests
{
    [TestInitialize]
    public async Task Initialize()
    {
        var app = await TestHelper.CreateWebApplication();
        await TestHelper.CleanupDatabase(app.Services);
    }

    [TestMethod]
    public async Task Import_From_File()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<IMediator>();

        var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

        var exercice = await mediator.Send(new Requests.CreateExerciceRequest($"Exercice {Guid.NewGuid()}", 
            "Exercice 2022", 
            8300, 
            8400, 
            100 * 1000000));

        var saveResult = await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
        saveResult.HasError.Should().BeFalse();

        var planFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "InitialAccountingPlan.json");
        var planFileContent = System.IO.File.ReadAllText(planFile);

        var plan = System.Text.Json.JsonSerializer.Deserialize<List<Datas.AccountData>>(planFileContent, ComptaClub.JsonSerializer.Options);

        var importResult = await mediator.Send(new ImportAccountingPlanRequest(plan!));
        importResult.HasError.Should().BeFalse();

        var bank = await mediator.Send(new CreateBankRequest("MyBank", "My Bank"));
        var saveBankResult = await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));
        saveBankResult.HasError.Should().BeFalse();

        var fileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"..\..\..\..\..\Doc\2022-11-11_14h53-releve_COMPTE_CHEQUES_1.ofx");
        var sr = System.IO.File.OpenRead(fileName);
        var ms = new MemoryStream();
        await sr.CopyToAsync(ms);

        var importedTransactionList = await mediator.Send(new ImportEntryListFromStreamRequest(ms));

        var importCount = importedTransactionList.Count();

        foreach (var import in importedTransactionList)
        {
            import.UserCreatorId = user.Id;
            var saveItemResult = await mediator.Send(new Requests.SaveEntityRequest<Datas.EntryData>(import));
            saveItemResult.HasError.Should().BeFalse();
        }

        var entries = await mediator.Send(new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(f =>
        {
            f.ComputeRowCount = Models.ComputeRowCount.InAllPages;
            f.PageSize = int.MaxValue;
        }));

        entries.Total.RowCount.Should().Be(importCount);  


    }
}
