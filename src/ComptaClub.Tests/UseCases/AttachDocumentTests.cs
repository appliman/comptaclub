using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Extensions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class AttachDocumentTests
{
	[TestMethod]
	public async Task Attach_Document_To_Entry()
	{
		var app = await TestHelper.CreateWebApplication();
		var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();

		var user = await mediator.GetOrCreateUser($"{Guid.NewGuid()}@email.com");

		var currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
		var pdftest = System.IO.Path.Combine(currentFolder, "docs", "test.pdf");

		var document = await mediator.Send(new CreateDocumentRequest());
		document.FileName = System.IO.Path.GetFileName(pdftest);
		document.MimeType = "application/pdf";

		var content = System.IO.File.ReadAllBytes(pdftest);
		var ms = new System.IO.MemoryStream(content);

		var saveResult = await mediator.Send(new SaveDocumentRequest(document, ms));
		saveResult.HasError.Should().BeFalse();

		var documentFilter = new DocumentListFilter();
		documentFilter.PageSize = int.MaxValue;
		documentFilter.GetById(saveResult.Id);

		var page = await mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(documentFilter));
		page.List.Should().HaveCount(1);

		var doc = page.List.First();
		doc.FileName.Should().Be("test.pdf");
		doc.MimeType.Should().Be("application/pdf");
		doc.Size.Should().Be(content.Length);
		doc.Id.Should().Be(saveResult.Id);

		var exercice = await mediator.GetOrCreateExercice($"{Guid.NewGuid()}");
		var bank = await mediator.GetOrCreateBank($"{Guid.NewGuid()}");
		var plan = await mediator.GetOrCreatePlan();
		var leafPlan = plan.GetLeafList();

		var licenceAccount = leafPlan.Single(i => i.Code == "756001");

		var entry = await mediator.CreateAndSaveRandomCreditEntry(exercice, user, bank, licenceAccount.Id, DateTime.Now);

		var entryDocument = await mediator.Send(new AttachDocumentToEntryRequest(entry!.Id, doc));
		entryDocument.Should().NotBeNull();
		entryDocument.HasError.Should().BeFalse();

		documentFilter = new DocumentListFilter();
		documentFilter.PageSize = int.MaxValue;
		documentFilter.MetaEntityIdList = new MetaEntityIdList(Enums.MetaEntity.Entry, new List<Guid> { entry.Id });

		page = await mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(documentFilter));
		page.List.Should().HaveCount(1);

		doc = page.List.First();
		doc.FileName.Should().Be("test.pdf");

		// Supression du document associé a l'entrée
		var removeResult = await mediator.Send(new RemoveDocumentFromEntryRequest(entry.Id, doc.Id));
		removeResult.Should().NotBeNull();

		page = await mediator.Send(new GetPagedEntityListRequest<DocumentListFilter, Datas.DocumentData>(documentFilter));
		page.List.Should().HaveCount(0);

	}
}
