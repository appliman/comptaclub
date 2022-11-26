using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFScriptableMigration;

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
    }
}
