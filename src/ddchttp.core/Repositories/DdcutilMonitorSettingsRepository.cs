using System;
using Microsoft.Extensions.Options;

namespace ddchttp.Repositories;

internal class DdcutilMonitorSettingsRepository : IMonitorSettingsRepository
{
	private readonly DdcutilMonitorSettingsRepositorySettings _settings;

	public DdcutilMonitorSettingsRepository(
		IOptions<DdcutilMonitorSettingsRepositorySettings> settings
	)
	{
		this._settings = settings.Value;
	}

	public async Task ModifyVcpAsync(VcpModificationArgs args)
	{
		var r = await CommandExecutorHelper.ExecuteAsync(new CommandExecutionArgs {
			ExecutablePaths = this._settings.CommandExecutionPaths
			, ExecutionArguments = this.BuildModifyVcpCommandArguments(args)
		});

		if(!r.Success)
		{
			throw new ApplicationException($"failed to modify vcp {r.ExitCode}: {r.StandardError}");
		}
	}

	public Task<VcpQueryResult> QueryVcpAsync(VcpQueryArgs args)
	{
		throw new NotImplementedException();
	}

	public string[] BuildModifyVcpCommandArguments(VcpModificationArgs args)
	{
		var arguments = new List<object>();

		arguments.Add("-d");
		arguments.Add(args.DisplayNumber);
		arguments.Add("setvcp");
		arguments.Add(args.FeatureCode);
		arguments.Add(args.Value);

		if (!string.IsNullOrWhiteSpace(args.I2cSourceAddress))
		{
			arguments.Add($"--i2c-source-addr={args.I2cSourceAddress}");
		}

		if (args.SkipVerification)
		{
			arguments.Add("--noverify");
		}		

		return arguments
			.Select(a => a.ToString())
			.ToArray()
		;
	}
}
