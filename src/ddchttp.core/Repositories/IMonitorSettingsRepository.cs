using System;

namespace ddchttp.Repositories;

public class VcpQueryArgs
{
	public int DisplayNumber { get; set; }

	public string FeatureCode { get; set; }
}

public class VcpQueryResult
{
	public string MH { get; set; }
	public string ML { get; set; }
	public string SH { get; set; }
	public string SL { get; set; }
}

public class VcpModificationArgs
{
	public int DisplayNumber { get; set; }

	public string FeatureCode { get; set; }

	public string Value { get; set; }

	public string I2cSourceAddress { get; set; }

	public bool SkipVerification { get; set; }
}

public interface IMonitorSettingsRepository
{
	Task<VcpQueryResult> QueryVcpAsync(VcpQueryArgs args);

	Task ModifyVcpAsync(VcpModificationArgs args);
}
