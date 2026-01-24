using System;

namespace ddchttp.Services;

public interface IDisplayProvider
{
	Task ModifyInputAsync(string name);
}
