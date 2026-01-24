using ddchttp.Services;

namespace ddchttp.Controllers;

public class HealthController
{

    public async Task<ControllerResult> GetAsync()
    {
        return await Task.FromResult(ControllerResult.Success());
    }
}
