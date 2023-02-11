using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds;

[TestClass]
public class MemberCrudTests
{
    [TestInitialize]
    public async Task Initialize()
    {
        var app = await TestHelper.CreateWebApplication();
        await TestHelper.CleanupDatabase(app.Services);
    }

    [TestMethod]
    public async Task Member_Crud()
    {
        var app = await TestHelper.CreateWebApplication();
        var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

        var member = await mediator.Send(new Requests.Members.GetMemberByFilterRequest(i => i.Email = "fake"));
        member.Should().BeNull();

        var memberEmail = $"{Guid.NewGuid()}@email.com";
        var memberName = $"{Guid.NewGuid()}";

        member = await mediator.Send(new Requests.Members.CreateMemberRequest());
        member.Name = memberName;
        member.Email = memberEmail;
        member.LicenseNumber = $"{Guid.NewGuid()}";

        var saveResult = await mediator.Send(new SaveEntityRequest<Datas.MemberData>(member));
        saveResult.HasError.Should().BeFalse();

        member = await mediator.Send(new Requests.Members.GetMemberByFilterRequest(i => i.LicenseNumber = member.LicenseNumber));
        member.Should().NotBeNull();

        member!.Name.Should().Be(memberName);

        memberName = member.Name = $"{Guid.NewGuid()}";
        saveResult = await mediator.Send(new SaveEntityRequest<Datas.MemberData>(member));
        saveResult.HasError.Should().BeFalse();

        member.Name.Should().Be(memberName);

        var memberList = await mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(f => f.PageSize = int.MaxValue));
        memberList.Should().NotBeNull();
        memberList.List.Should().NotBeNull();
        memberList.List.Any().Should().BeTrue();
    }
}
