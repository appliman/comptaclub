using System.Runtime.CompilerServices;

using EFScriptableMigration;

using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Datas;

public static class StartupExtensions
{
	public static IServiceCollection AddComptaClubDbContext(this IServiceCollection services, Action<DbConfiguration> dbConfiguration)
	{
        var cfg = new DbConfiguration();
        dbConfiguration.Invoke(cfg);

		services.AddDbContextFactory<ComptaClubDbContext>(lifetime: ServiceLifetime.Singleton);
		services.AddSingleton(cfg);
		return services;
	}
}
