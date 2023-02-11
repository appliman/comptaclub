using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;

using FluentAssertions;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds;

[TestClass]
public class DocumentCrudTests
{
    [TestInitialize]
    public async Task Initialize()
    {
        var app = await TestHelper.CreateWebApplication();
        await TestHelper.CleanupDatabase(app.Services);
    }

    [TestMethod]
    public async Task Document_Crud()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();


        var document = await mediator.Send(new Requests.Documents.CreateDocumentRequest());
        document.Should().NotBeNull();

        var saveResult = await mediator.Send(new Requests.Documents.SaveDocumentRequest(document));
        saveResult.Should().NotBeNull();    
        saveResult.HasError.Should().BeTrue();

        var fileName = document.FileName = TestHelper.GetRandomName();
        var mimeType = document.MimeType= "text/plain";
        var description = document.Description = TestHelper.GetRandomName();

        var textContent = TestHelper.GetRandomName();
        var content = System.Text.Encoding.Default.GetBytes(textContent);

        saveResult = await mediator.Send(new Requests.Documents.SaveDocumentRequest(document, content));
        saveResult.Should().NotBeNull();
        saveResult.HasError.Should().BeFalse();

        document = await mediator.Send(new Requests.Documents.GetDocumentByFilterRequest(f => f.GetById(document.Id)));
        document.Should().NotBeNull();

        document!.FileName.Should().Be(fileName);
        document.MimeType.Should().Be(mimeType);
        document.Size.Should().BeGreaterThan(0);
        document.Description.Should().Be(description);

        var fileName2 = document.FileName = TestHelper.GetRandomName();
        saveResult = await mediator.Send(new Requests.Documents.SaveDocumentRequest(document));
        saveResult.Should().NotBeNull();
        saveResult.HasError.Should().BeFalse();

        var ms = new MemoryStream();
        var sizeOfContent = await mediator.Send(new Requests.Documents.GetDocumentContentRequest(document!.Id, ms));
        sizeOfContent.Should().BeGreaterThan(0);

        var dbContent = System.Text.Encoding.Default.GetString(ms.ToArray());
        dbContent.Should().Be(textContent);

        document = await mediator.Send(new Requests.Documents.GetDocumentByFilterRequest(f => f.GetById(document.Id)));
        document.Should().NotBeNull();

        document!.FileName.Should().Be(fileName2);

        var documentPage = await mediator.Send(new Requests.GetPagedEntityListRequest<DocumentListFilter, DocumentData>(f =>
        {
            f.PageSize = int.MaxValue;
        }));

        documentPage.Should().NotBeNull();
        documentPage.List.Any().Should().BeTrue();
        documentPage.List.Count().Should().Be(1);

        var deleteResult = await mediator.Send(new Requests.Documents.DeleteDocumentRequest(document.Id));
        deleteResult.Should().NotBeNull();  
        deleteResult.HasError!.Should().BeFalse();
        deleteResult.ChangeCount.Should().Be(2);

        documentPage = await mediator.Send(new Requests.GetPagedEntityListRequest<DocumentListFilter, DocumentData>(f =>
        {
            f.PageSize = int.MaxValue;
        }));
        documentPage.Should().NotBeNull();
        documentPage.List.Any().Should().BeFalse();

    }
}
