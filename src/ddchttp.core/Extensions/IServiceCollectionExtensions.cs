using Microsoft.Extensions.DependencyInjection.Extensions;
using ddchttp.Services;
using ddchttp.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddDdcutilMonitorSettingsRepository(
		this IServiceCollection services
		, Action<DdcutilMonitorSettingsRepositorySettings> settingsBuilder
	)
	{
		services.Configure(settingsBuilder);

		services.TryAddTransient<IMonitorSettingsRepository, DdcutilMonitorSettingsRepository>();

		return services;
	}

	public static IServiceCollection AddLg45GX950ADisplayProvider(
		this IServiceCollection services
		, Action<Lg45GX950ADisplayProviderSettings> settingsBuilder
	)
	{
		services.Configure(settingsBuilder);

		services.TryAddTransient<IDisplayProvider, Lg45GX950ADisplayProvider>();

		return services;
	}	
}
