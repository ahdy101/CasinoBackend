using Casino.Backend.Repositories.Interfaces;
using System.Security.Claims;

namespace Casino.Backend.Middleware
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityTrackingMiddleware> _logger;

    public ActivityTrackingMiddleware(RequestDelegate next, ILogger<ActivityTrackingMiddleware> logger)
        {
     _next = next;
   _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
   // Execute the request first
            await _next(context);

   // After response, update user activity (fire and forget)
  _ = Task.Run(async () =>
  {
    try
       {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

              if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
     {
      await userRepository.UpdateLastActivityAsync(userId);
        }
       }
      catch (Exception ex)
   {
  _logger.LogError(ex, "Error updating user activity");
              }
  });
        }
    }
}
