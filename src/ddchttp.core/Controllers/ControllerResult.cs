namespace ddchttp.Controllers;

public enum ControllerResultStatuses
{
    Success,
    Error,
    ValidationFailure,
    ExistanceFailure
}

public record ControllerResult(
    ControllerResultStatuses Status
    , string Message = null
)
{
    public static ControllerResult Success() => new(ControllerResultStatuses.Success, null);
    public static ControllerResult Failure(string message = null) => new(ControllerResultStatuses.Error, message);
    public static ControllerResult ValidationFailure(string message = null) => new(ControllerResultStatuses.ValidationFailure, message);
    public static ControllerResult ExistanceFailure() => new(ControllerResultStatuses.ExistanceFailure, null);
}
