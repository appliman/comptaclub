
using ComptaClub.Datas;

using FluentValidation;

using ChannelMediator;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ComptaClub.Configuration;

namespace ComptaClub;

public static class StartupExtensions
{
    public static Configuration.ComptaClubSettings ConfigureComptaClub(this WebApplicationBuilder builder, params string[] args)
    {
        builder.Configuration.SetBasePath(builder.Environment.ContentRootPath);
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
        }
        builder.Configuration.AddEnvironmentVariables();

        var section = builder.Configuration.GetSection("ComptaClub");
        var settings = new Configuration.ComptaClubSettings();
        section.Bind(settings);
        builder.Services.AddSingleton(settings);

		if (settings.DatabaseProvider.Equals("MsSql", StringComparison.OrdinalIgnoreCase))
        {
            var sqlConnectionString = args.GetParameterValue("cs");
            settings.ConnectionString = string.IsNullOrWhiteSpace(sqlConnectionString)
                ? settings.ConnectionString
                : sqlConnectionString;
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException("ComptaClub:ConnectionString is required for MsSql.");
            }
        }

        builder.Services.AddComptaClubCore();

        builder.Services.AddMemoryCache();

        if (builder.Environment.IsDevelopment())
        {
			builder.Logging.AddDebug();
			builder.Logging.AddConsole();
		}

		return settings;
    }

    public static IServiceCollection AddComptaClubCore(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ComptaClub.Security.McpApiKeyService>();
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddScoped<ComptaClub.Contracts.Models.ApiKeys.ICurrentApplicationUser, ComptaClub.Security.AnonymousApplicationUser>(services);
		var rootNs = typeof(StartupExtensions).Namespace!.Split('.')[0];
		var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => a.FullName!.StartsWith(rootNs))
				.ToArray();

		services.AddChannelMediator(
            config => config.Strategy = NotificationPublishStrategy.Sequential,
			currentAssemblies);

		services.AddValidatorsFromAssemblies(currentAssemblies, includeInternalTypes: true, lifetime: ServiceLifetime.Singleton);

        return services;
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
