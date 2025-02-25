using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Contracts.Interfaces;
using ProjectsLoader.Services;

public class CheckUserMiddleware
{
    private readonly RequestDelegate _next;

    public CheckUserMiddleware(RequestDelegate next)
    {
        _next = next;
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
            
            var counterService = context.RequestServices.GetRequiredService<RedisService>();
            var activeUsers = await counterService.GetAsync<HashSet<string>>("ActiveUsers");

            if (userName != null && activeUsers != null && !activeUsers.Contains(userName))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: User not active.");
            }
        }
    }
}