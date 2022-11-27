using ComptaClub.Datas;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests.Cruds
{
    [TestClass]
    public class EntryCrudTests
    {
        [TestMethod]
        public async Task Entry_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var entry = await mediator.Send(new GetEntryByFilterRequest(i => i.Id == Guid.NewGuid()));
            entry.Should().BeNull();

            entry = await mediator.Send(new CreateEntryRequest());
            entry.Should().NotBeNull();

            var saveResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));
            saveResult.HasError.Should().BeTrue();
        }

    }
}