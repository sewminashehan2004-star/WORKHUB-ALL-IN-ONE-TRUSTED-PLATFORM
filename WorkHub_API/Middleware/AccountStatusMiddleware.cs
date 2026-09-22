using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;

namespace WorkHub.API.Middleware
{
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountStatusMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ApplicationDbContext dbContext)
        {
            // User is not logged in
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var userIdValue =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        message =
                            "Invalid authentication token."
                    });

                return;
            }

            var user =
                await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (user == null)
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        message =
                            "User account could not be found."
                    });

                return;
            }

            if (!user.AccountStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        message =
                            "Your account is currently suspended."
                    });

                return;
            }

            await _next(context);
        }
    }
}