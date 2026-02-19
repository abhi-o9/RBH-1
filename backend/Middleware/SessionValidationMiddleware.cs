using System.Security.Claims;
using backend.Services;
using backend.Models;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CouchDbService couchDb)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var email = context.User.Identity?.Name;
            var tokenSessionId = context.User.FindFirst("sessionId")?.Value;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(tokenSessionId))
            {
                var dbUser = await couchDb.GetUserByEmailAsync(email);

                if (dbUser == null || dbUser.currentSessionId != tokenSessionId)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Session invalid or logged in elsewhere.");
                    return;
                }
            }
        }

        await _next(context);
    }

}
