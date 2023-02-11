using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Extensions;
using ComptaClub.Models;
using ComptaClub.Requests;

using FluentAssertions;

using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.UseCases;

[TestClass]
public class ImportMemberListFromExcelTests
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

        var fileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, @"..\..\..\..\..\Doc\export_excel_saison.xlsx");
        var import = await mediator.Send(new Requests.Members.ImportExcelMemberListRequest(fileName));

        import.Should().NotBeNull();

        var memberList = await mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(f => f.PageSize = int.MaxValue));
        memberList.Should().NotBeNull();
        memberList.List.Should().NotBeNull();
        memberList.List.Any().Should().BeTrue();

    }
}
