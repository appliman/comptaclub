using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

namespace ComptaClub.Tests
{
    public static class TestHelper
    {
        public async static Task<WebApplication> CreateWebApplication()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";

            var settings = await builder.ConfigureComptaClub();

            var app = builder.Build();

            return app;
        }
    }
}
