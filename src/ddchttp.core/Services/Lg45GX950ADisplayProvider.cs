using System;
using ddchttp.Repositories;
using Microsoft.Extensions.Options;

namespace ddchttp.Services;

internal class Lg45GX950ADisplayProvider : IDisplayProvider
{
	private readonly IMonitorSettingsRepository _repository;
	private readonly Lg45GX950ADisplayProviderSettings _settings;

	public Lg45GX950ADisplayProvider(
		IMonitorSettingsRepository repository
		, IOptions<Lg45GX950ADisplayProviderSettings> settings
	)
	{
		this._repository = repository;
		this._settings = settings.Value;
	}

	public async Task ModifyInputAsync(string name)
	{
		var code = this.MapNameToCode(name);

		var args = new VcpModificationArgs {
			DisplayNumber = 1
			, FeatureCode = "0xf4"
			, Value = code
			, I2cSourceAddress = "0x50"
			, SkipVerification = false
		};

		await this._repository.ModifyVcpAsync(args);
	}

	public string MapNameToCode(string name)
	{
		return name.ToLower() switch {
			"hdmi1" => "0x90"
			, "hdmi2" => "0x91"
			, "dp" => "0xd0"
			, "usbc" => "0xd1"
			, _ => throw new ArgumentOutOfRangeException(
				nameof(name)
				, $"value of '{name}' is not supported."
			)
		};
	}
}
