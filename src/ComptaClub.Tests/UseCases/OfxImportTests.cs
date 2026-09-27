using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using System.Text;

using FluentAssertions;

using ChannelMediator;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class OfxImportTests
{
	[TestMethod]
	public async Task Import_From_File()
	{
		await using var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<IMediator>();

		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var exercice = await mediator.Send(new CreateExerciceRequest($"Exercice {Guid.NewGuid()}",
			"Exercice 2022",
			100 * 1000000));

		exercice.StartDate = 8000;
		exercice.EndDate = 8600;

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

		const string OFX_CONTENT = """
OFXHEADER:100
DATA:OFXSGML
VERSION:102
SECURITY:NONE
ENCODING:USASCII
CHARSET:1252
COMPRESSION:NONE
OLDFILEUID:NONE
NEWFILEUID:NONE

<OFX>
<SIGNONMSGSRSV1><SONRS><STATUS><CODE>0<SEVERITY>INFO</STATUS><DTSERVER>20221111145300<LANGUAGE>ENG</SONRS></SIGNONMSGSRSV1>
<BANKMSGSRSV1><STMTTRNRS><TRNUID>1<STATUS><CODE>0<SEVERITY>INFO</STATUS><STMTRS>
<CURDEF>EUR
<BANKACCTFROM><BANKID>12345<ACCTID>123456789<ACCTTYPE>CHECKING</BANKACCTFROM>
<BANKTRANLIST><DTSTART>20221101000000<DTEND>20221111000000
<STMTTRN><TRNTYPE>DEBIT<DTPOSTED>20221111120000<TRNAMT>-12.34<FITID>TEST-OFX-001<NAME>Cotisation<MEMO>Import de test</STMTTRN>
</BANKTRANLIST>
<LEDGERBAL><BALAMT>0.00<DTASOF>20221111145300</LEDGERBAL>
</STMTRS></STMTTRNRS></BANKMSGSRSV1>
</OFX>
""";
		using var ms = new MemoryStream();
		await ms.WriteAsync(Encoding.UTF8.GetBytes(OFX_CONTENT));
		ms.Position = 0;

		var importedTransactionList = await mediator.Send(new ImportEntryListFromStreamRequest(ms));

		var importCount = importedTransactionList.Count();
		importCount.Should().Be(1);

		foreach (var import in importedTransactionList)
		{
			import.UserCreatorId = user.Id;
			var saveItemResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(import));
			saveItemResult.HasError.Should().BeFalse();
		}

		var entries = await mediator.Send(new GetPagedEntityListRequest<EntryListFilter, Datas.EntryData>(f =>
		{
			f.ComputeRowCount = ComputeRowCount.InAllPages;
			f.PageSize = int.MaxValue;
		}));

		entries.Total.RowCount.Should().Be(importCount);


	}
}
