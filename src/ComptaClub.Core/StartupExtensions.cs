using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ComptaClub;

public static class StartupExtensions
{
    public static async Task<Configuration.ComptaClubSettings> ConfigureComptaClub(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appSettings.json");
        builder.Configuration.AddJsonFile($"appSettings.{builder.Environment.EnvironmentName}.json");

        var section = builder.Configuration.GetSection("ComptaClub");
        var settings = new Configuration.ComptaClubSettings();
        section.Bind(settings);

        builder.Services.AddSingleton(settings);

        builder.Services.AddScoped<Services.ITableStorageService, Services.TableStorageService>();
        builder.Services.AddAutoMapper(typeof(StartupExtensions));
        builder.Services.AddMediatR(typeof(StartupExtensions));

        builder.Services.AddTransient<IValidator<Models.Bank>, Validators.BankValidator>();
        builder.Services.AddTransient<IValidator<Models.Account>, Validators.AccountValidator>();
        builder.Services.AddTransient<IValidator<Models.Exercice>, Validators.ExerciceValidator>();

        return await Task.FromResult(settings);
    }
}
