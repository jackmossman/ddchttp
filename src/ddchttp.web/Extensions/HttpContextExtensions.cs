using System.Linq;

namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static string GetRemoteIp(this HttpContext context)
    {
        var xff = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(xff))
        {
            return xff.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
