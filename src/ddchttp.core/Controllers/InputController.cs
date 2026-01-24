using Microsoft.Extensions.Options;
using ddchttp.Services;

namespace ddchttp.Controllers;

public class InputController
{
    private readonly IDisplayProvider _displayProvider;
    private readonly InputControllerSettings _settings;

    public InputController(
        IDisplayProvider displayProvider
        , IOptions<InputControllerSettings> options
    )
    {
        _displayProvider = displayProvider;
        _settings = options.Value;
    }

    public async Task<ControllerResult> SwapAsync(string type, string remoteIp)
    {
        var validTypes = this.ExtractValidSwapTypes();

        if (!validTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
        {
            return ControllerResult.ValidationFailure("invalid swap type");
        }

        if (string.IsNullOrEmpty(remoteIp))
        {
            return ControllerResult.ValidationFailure("missing remote ip");
        }

        var name = this.QueryNameBySwapAndIp(type, remoteIp);

        if (name == null)
        {
            return ControllerResult.ValidationFailure("no match found for swap type and remote ip");
        }

        try
        {
            await _displayProvider.ModifyInputAsync(name);

            return ControllerResult.Success();
        }
        catch (Exception ex)
        {
            return ControllerResult.Failure(ex.Message);
        }
    }

    public async Task<ControllerResult> SetAsync(string name)
    {
        var validNames = this.ExtractValidNames();
    
        if (!validNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return ControllerResult.ValidationFailure("invalid input name");
        }

        try
        {
            await this._displayProvider.ModifyInputAsync(name);

            return ControllerResult.Success();
        }
        catch (Exception ex)
        {
            return ControllerResult.Failure(ex.Message);
        }
    }

    public string[] ExtractValidSwapTypes()
    {
        return (_settings.AvailableInputs ?? [])
			.Where(x => x.Swap != null)
			.SelectMany(x => x.Swap)
			.Where(x => !string.IsNullOrEmpty(x))
			.Distinct()
			.ToArray()
        ;
    }

    public string[] ExtractValidNames()
    {
        return (_settings.AvailableInputs ?? [])
			.Select(x => x.Name)
			.Where(x => !string.IsNullOrEmpty(x))
			.Distinct()
			.ToArray()
        ;
    }

    public string QueryNameBySwapAndIp(string swap, string remoteIp)
    {
        return (_settings.AvailableInputs ?? [])
            .Where(x => 
                x.Swap != null
                && x.Ip != null
            )
            .Where(x => x.Swap.Contains(swap))
            .FirstOrDefault(x => x.Ip != remoteIp)
            ?.Name
        ;
    }
}
