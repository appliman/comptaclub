using ComptaClub.Datas;
using ComptaClub.Requests;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
    [TestClass]
    public class EntryCrudTests
    {
        [TestMethod]
        public async Task Entry_Crud()
        {
            var app = await TestHelper.CreateWebApplication();
            var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

            var entry = await mediator.Send(new Requests.GetEntryByIdRequest(Guid.NewGuid()));
            entry.Should().BeNull();

            entry = await mediator.Send(new Requests.CreateEntryRequest());
            entry.Should().NotBeNull();

            var saveResult = await mediator.Send(new SaveEntryRequest(entry));
            saveResult.HasError.Should().BeTrue();  


        }

    }
}