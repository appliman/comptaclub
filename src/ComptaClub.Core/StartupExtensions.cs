using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using ComptaClub.Datas;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComptaClub;

public static class StartupExtensions
{
    public static async Task<(Configuration.ComptaClubSettings settings, TokenCredential credential)> ConfigureComptaClub(this WebApplicationBuilder builder, string? sqlConnectionString = null)
    {
        var currentFolder = System.IO.Path.GetDirectoryName(typeof(StartupExtensions).Assembly.Location);
        builder.Configuration
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json")
            .AddJsonFile($"appsettings.local.json", true)
            .AddEnvironmentVariables()
            .SetBasePath(currentFolder!);

        var section = builder.Configuration.GetSection("ComptaClub");
        var settings = new Configuration.ComptaClubSettings();
        section.Bind(settings);
        builder.Services.AddSingleton(settings);

        var vaultUri = new Uri($"https://{settings.KeyVaultName}.vault.azure.net");
        var credential = new ClientSecretCredential(settings.KeyVaultTenantId, settings.KeyVaultClientId, settings.KeyVaultClientSecret);
        var client = new SecretClient(vaultUri,credential);

        var csSecret = await client.GetSecretAsync("AzureStorageConnectionString");
        settings.SetAzureStorageConnectionString(csSecret.Value.Value);

        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            var cs = await client.GetSecretAsync("SqlConnectionString");
            settings.SetSqlConnectionString(cs.Value.Value);
        }
        else
        {
            settings.SetSqlConnectionString(sqlConnectionString);
        }

        var azureStorageAccountKey = await client.GetSecretAsync("AzureStorageAccountKey");
        settings.SetAzureStorageAccountKey(azureStorageAccountKey.Value.Value);

        builder.Services.AddComptaClubDbContext(cfg =>
        {
            cfg.EnvironmentName = builder.Environment.EnvironmentName;
            cfg.ConnectionString = settings.SqlConnectionString;
        });

        builder.Services.AddScoped<Services.IAccountingService, Services.AccountingService>();

        builder.Services.AddAutoMapper(typeof(StartupExtensions));
        builder.Services.AddMediatR(typeof(StartupExtensions));

        builder.Services.AddTransient<IValidator<Models.Bank>, Validators.BankValidator>();
        builder.Services.AddTransient<IValidator<Models.Account>, Validators.AccountValidator>();
        builder.Services.AddTransient<IValidator<Models.Exercice>, Validators.ExerciceValidator>();
        builder.Services.AddTransient<IValidator<Models.Entry>, Validators.EntryValidator>();

        builder.Services.AddMemoryCache();

        if (builder.Environment.IsDevelopment())
        {
			builder.Logging.AddDebug();
			builder.Logging.AddConsole();
		}

		return await Task.FromResult((settings, credential));
    }
}
