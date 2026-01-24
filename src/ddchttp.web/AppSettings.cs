namespace ddchttp.web;

public class AppSiteSettings
{
	public int Port { get; set; } = 8080;
}

public class AppSettings
{
	public AppSiteSettings Site { get; set; } = new AppSiteSettings();
	public List<AvailableInput> AvailableInputs { get; set; } = new();
}
