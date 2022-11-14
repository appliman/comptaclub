using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Datas;

using FluentAssertions;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
    [TestClass]
    public class AccountingPlanTests
    {
        [TestMethod]
        public async Task Create_Plan()
        {
            var app = await TestHelper.CreateWebApplication();
            var planService = app.Services.GetRequiredService<Services.AccountingService>();

            await planService.CreateOrSynchronize();
        }

		[TestMethod]
		public async Task Get_Plan()
        {
			var app = await TestHelper.CreateWebApplication();
			var planService = app.Services.GetRequiredService<Services.IAccountingService>();

            var plan = await planService.GetAccountingPlan();

            var json = System.Text.Json.JsonSerializer.Serialize(plan, options: ComptaClub.JsonSerializer.Options);

		}
	}
}
