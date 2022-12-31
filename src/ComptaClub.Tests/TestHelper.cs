using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;

using EFScriptableMigration;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ComptaClub.Tests
{
    public static class TestHelper
    {
        public async static Task<WebApplication> CreateWebApplication()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";

            var cs = builder.Configuration.GetConnectionString("TEST");

            var dbTest = new Datas.ComptaClubDbContext(new Datas.DbConfiguration
            {
                ConnectionString = cs!,
                EnvironmentName = builder.Environment.EnvironmentName
            });
            await dbTest.Database.EnsureDeletedAsync();
            await dbTest.Database.EnsureCreatedAsync();

            var cfg = await builder.ConfigureComptaClub(cs!);

            var migration = new DbMigration()
            {
                ConnectionString = cfg.settings.SqlConnectionString,
                SchemaName = "ComptaClub",
                EmbededTypeReference = typeof(StartupExtensions)
            };

            await migration.Start();

            var app = builder.Build();

            return app;
        }

        public async static Task<List<Datas.AccountData>> GetOrCreatePlan(IMediator mediator)
        {
            var planFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "InitialAccountingPlan.json");
            var planFileContent = System.IO.File.ReadAllText(planFile);

            var plan = System.Text.Json.JsonSerializer.Deserialize<List<Datas.AccountData>>(planFileContent, ComptaClub.JsonSerializer.Options);

            await mediator.Send(new ImportAccountingPlanRequest(plan!));

            return plan!;
        }

        public async static Task<Datas.BankData> GetOrCreateBank(string bankName, IMediator mediator)
        {
            var bank = await mediator.Send(new GetBankByFilterRequest(i => i.Code == bankName));
            if (bank == null)
            {
                bank = await mediator.Send(new CreateBankRequest("MyBank", "My Bank"));
                await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));
            }
            return bank;
        }
    }
}
