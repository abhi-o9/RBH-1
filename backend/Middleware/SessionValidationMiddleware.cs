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
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var email = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var tokenSessionId = context.User.FindFirst("sessionId")?.Value;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(tokenSessionId))
            {
                var userId = $"user:{email}";
                var dbUser = await couchDb.GetAsync<User>(userId);

                if (dbUser == null || dbUser.currentSessionId != tokenSessionId)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Session expired.");
                    return;
                }
            }
        }

        await _next(context);
    }

}
