using System.Diagnostics;
using System.Net;
using System.Threading.RateLimiting;
using ddchttp.Controllers;
using Microsoft.AspNetCore.RateLimiting;

namespace ddchttp.web;

public class Program
{
	public static async Task Main(string[] args)
	{
		await new Program().ExecuteAsync(args);
	}

	public async Task ExecuteAsync(string[] args)
	{
		var builder = WebApplication.CreateSlimBuilder(args);

		builder.Configuration.AddJsonFile("settings.json", optional: false, reloadOnChange: true);
		builder.Configuration.AddEnvironmentVariables();
		builder.Services.Configure<AppSettings>(builder.Configuration);

		builder.Services.ConfigureHttpJsonOptions(options => {
			options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
		});

		builder.Services.Configure<InputControllerSettings>(
			builder.Configuration.GetSection("Controllers:Input")
		);

		builder.Services.AddTransient<HealthController>();
		builder.Services.AddTransient<InputController>();

		builder.Services.AddDdcutilMonitorSettingsRepository(settings => {
			builder.Configuration.GetSection("Repositories:MonitorSettings:Ddcutil").Bind(settings);
		});

		builder.Services.AddLg45GX950ADisplayProvider(settings => {
			builder.Configuration.GetSection("Services:DisplayProviders:Lg45GX950A").Bind(settings);
		});

		builder.Services.AddRateLimiter(o => {
			o.AddConcurrencyLimiter("single", oo => {
				oo.PermitLimit = 1;
				oo.QueueLimit = 20;
				oo.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			});
		});

		var webapp = builder.Build();

		webapp.MapGet("/health", async (HealthController c) => await c.GetAsync().ToHttpAsync());

		webapp
			.MapPost("/input/{name}", async (string name, InputController c) => await c.SetAsync(name).ToHttpAsync())
			.RequireRateLimiting("single")
		;

		webapp
			.MapPost("/input/swap/{type}", async (string type, HttpContext ctx, InputController c) => {
				var remoteIp = ctx.GetRemoteIp();

				return await c.SwapAsync(type, remoteIp).ToHttpAsync();
			})
			.RequireRateLimiting("single")
		;

		var appSettings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();

		await webapp.RunAsync($"http://0.0.0.0:{appSettings.Site.Port}");
	}
}
