using Azure.Data.Tables;
using ComptaClub.Datas;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
    [TestClass]
    public class BankCrudTests
    {
        [TestMethod]
        public async Task Bank_Crud()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";

            var settings = await builder.ConfigureComptaClub();

            var app = builder.Build();

            var accountingService = app.Services.GetService<Services.AccountingService>();

            var bank = await accountingService!.GetBankByName("fake");
            bank.Should().BeNull();

            var name = $"Bank{Guid.NewGuid()}";
            bank = accountingService!.CreateBank(name);

            var saveResult = await accountingService.SaveBank(bank);
            saveResult.HasError.Should().BeFalse();

            bank = await accountingService.GetBankByName(name);
            bank.Should().NotBeNull();

            bank.Label = $"{Guid.NewGuid()}";

            saveResult = await accountingService.SaveBank(bank);
            saveResult.HasError.Should().BeFalse();  
        }


        [TestMethod]
        public async Task Account_Crud()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";

            var settings = await builder.ConfigureComptaClub();

            var app = builder.Build();

            var accountingService = app.Services.GetService<Services.AccountingService>();

            var code = $"Test{Guid.NewGuid()}";
            var account = accountingService!.CreateAccount(code);
            account.Label= "Test";
            account.Direction = AccountDirection.Debit;

            await accountingService.SaveAccount(account);

            account.Label = account.Label + $"{Guid.NewGuid()}";

            await accountingService.SaveAccount(account);
        }
    }
}