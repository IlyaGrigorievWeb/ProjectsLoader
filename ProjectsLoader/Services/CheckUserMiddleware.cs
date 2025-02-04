using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Contracts.Interfaces;

public class CheckUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IActiveUserCounter _activeUserCounter;

    public CheckUserMiddleware(RequestDelegate next, IActiveUserCounter activeUserCounter)
    {
        _next = next;
        _activeUserCounter = activeUserCounter;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
        }
        else if (context.Request.Path.StartsWithSegments("/Indentity"))
        {
            await _next(context);
        }
        else
        {
            await _next(context);

            var userName = context.User?.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: User not found.");
            }

            var activeUsers = _activeUserCounter.GetActiveUser();

            if (!activeUsers.Contains(userName))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: User not active.");
            }
        }
    }
}