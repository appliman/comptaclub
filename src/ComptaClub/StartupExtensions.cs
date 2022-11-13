using Microsoft.Extensions.DependencyInjection;

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

        builder.Services.AddTransient<Services.AccountingService>();
        builder.Services.AddAutoMapper(typeof(Program));

        return await Task.FromResult(settings);
    }
}
