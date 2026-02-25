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
    public static async Task<Configuration.ComptaClubSettings> ConfigureComptaClub(this WebApplicationBuilder builder, params string[] args)
    {
		var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";
		builder.Environment.EnvironmentName = env;

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
        var credential = new ClientCertificateCredential(settings.KeyVaultTenantId, settings.KeyVaultClientId, settings.KeyVaultCertificatePath);
        var client = new SecretClient(vaultUri,credential);

        var csSecret = await client.GetSecretAsync("AzureStorageConnectionString");
        settings.SetAzureStorageConnectionString(csSecret.Value.Value);

        var sqlConnectionString = args.GetParameterValue("cs");
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

        var smtpPassword = await client.GetSecretAsync("SmtpPassword");
        settings.SetSmtpPassword(smtpPassword.Value.Value);

        builder.Services.AddComptaClubDbContext(cfg =>
        {
            cfg.EnvironmentName = builder.Environment.EnvironmentName;
            cfg.ConnectionString = settings.SqlConnectionString;
        });

        builder.Services.AddMediatR(typeof(StartupExtensions));

        builder.Services.AddTransient<IValidator<Datas.BankData>, Validators.BankValidator>();
        builder.Services.AddTransient<IValidator<Datas.AccountData>, Validators.AccountValidator>();
        builder.Services.AddTransient<IValidator<Datas.ExerciceData>, Validators.ExerciceValidator>();
        builder.Services.AddTransient<IValidator<Datas.EntryData>, Validators.EntryValidator>();
        builder.Services.AddTransient<IValidator<Datas.UserData>, Validators.UserValidator>();
        builder.Services.AddTransient<IValidator<Datas.MemberData>, Validators.MemberValidator>();
        builder.Services.AddTransient<IValidator<Datas.AssociatedMemberListByEntryData>, Validators.AssociatedMemberListByEntryValidator>();
        builder.Services.AddTransient<IValidator<Datas.DocumentData>, Validators.DocumentValidator>();

        builder.Services.AddMemoryCache();

        if (builder.Environment.IsDevelopment())
        {
			builder.Logging.AddDebug();
			builder.Logging.AddConsole();
		}

		return settings;
    }


    public static string GetParameterValue(this string[] args, string parameterName)
    {
        if (args == null
            || !args.Any())
        {
            return string.Empty;
        }

        string value = string.Empty;
        var nextisvalue = false;
        foreach (var item in args)
        {
            if (nextisvalue)
            {
                value = item;
                break;
            }
            if (item.Equals($"--{parameterName}", StringComparison.InvariantCultureIgnoreCase))
            {
                nextisvalue = true;
            }
        }
        return $"{value}".Trim();
    }
}
