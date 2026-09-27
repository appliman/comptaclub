using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Members;
using ClosedXML.Excel;

using FluentAssertions;

using ChannelMediator;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class ImportMemberListFromExcelTests
{
	[TestMethod]
	public async Task Import_From_File()
	{
		await using var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<IMediator>();

		using var workbook = new XLWorkbook();
		var worksheet = workbook.AddWorksheet("Membres");
		worksheet.Cell(1, 1).Value = "Numéro de licence";
		worksheet.Cell(1, 2).Value = "Prénom";
		worksheet.Cell(1, 3).Value = "Nom";
		worksheet.Cell(1, 4).Value = "Email";
		worksheet.Cell(1, 5).Value = "Type de licence";
		worksheet.Cell(2, 1).Value = "LIC-001";
		worksheet.Cell(2, 2).Value = "Alice";
		worksheet.Cell(2, 3).Value = "Martin";
		worksheet.Cell(2, 4).Value = "alice@example.com";
		worksheet.Cell(2, 5).Value = "Compétition";
		using var content = new MemoryStream();
		workbook.SaveAs(content);
		content.Position = 0;
		var import = await mediator.Send(new ImportExcelMemberListRequest(content));

		import.Should().NotBeNull();

		var memberList = await mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(f => f.PageSize = int.MaxValue));
		memberList.Should().NotBeNull();
		memberList.List.Should().NotBeNull();
		memberList.List.Should().ContainSingle(i => i.LicenseNumber == "LIC-001" && i.Name == "Alice Martin");

	}
}
