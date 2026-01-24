using System;
using System.Diagnostics;
using System.Linq;

namespace ddchttp.Repositories;

internal class CommandExecutionArgs
{
	public string[] ExecutablePaths { get; set; }
	public string[] ExecutionArguments { get; set; }
	public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(10);
}

internal class CommandExecutionResult
{
	public bool Success { get { return ExitCode == 0; } }
	public int ExitCode { get; set; }
	public string StandardOutput { get; set; }
	public string StandardError { get; set; }
}

internal static class CommandExecutorHelper
{
	public static async Task<CommandExecutionResult> ExecuteAsync(CommandExecutionArgs args)
	{
		if(args == null)
		{
			throw new ArgumentNullException(nameof(args));
		}

		if(args.ExecutablePaths == null || !args.ExecutablePaths.Any())
		{
			throw new ArgumentNullException(nameof(CommandExecutionArgs.ExecutablePaths));
		}

		var path = args.ExecutablePaths.FirstOrDefault(File.Exists);

		if(path == null)
		{
			throw new FileNotFoundException("executable not found in any of the provided paths");
		}

		var fileName = Path.GetFileName(path);
		var folder = Path.GetDirectoryName(path);

		using var cts = new CancellationTokenSource(args.ExecutionTimeout);

		var psi = new ProcessStartInfo {
			FileName = fileName,
			Arguments = (
				args.ExecutionArguments != null 
				? string.Join(" ", args.ExecutionArguments) 
				: string.Empty
			),
			WorkingDirectory = folder,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = Process.Start(psi);

		if (process == null)
		{
			throw new InvalidOperationException("failed to start process");
		}

		await process.WaitForExitAsync(cts.Token);

		return new CommandExecutionResult {
			ExitCode = process.ExitCode,
			StandardOutput = await process.StandardOutput.ReadToEndAsync(),
			StandardError = await process.StandardError.ReadToEndAsync()
		};
	}	
}
