using Microsoft.AspNetCore.Http;
using ddchttp.web;

namespace ddchttp.Controllers;

public static class ControllerResultExtensions
{
    public static IResult ToHttp(this ControllerResult result)
    {
        return result.Status switch {
            ControllerResultStatuses.Success => Results.NoContent(),
            ControllerResultStatuses.ValidationFailure => Results.BadRequest(new ErrorResult { Message = result.Message }),
            ControllerResultStatuses.ExistanceFailure => Results.NotFound(),
            _ => Results.InternalServerError(new ErrorResult{ Message = result.Message})
        };
    }

    public static async Task<IResult> ToHttpAsync(this Task<ControllerResult> result)
    {
        var r = await result;

        return ToHttp(r);
    }    
}
