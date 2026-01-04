using System.Collections.Concurrent;
using System.Net;

namespace Casino.Backend.Middleware
{
    /// <summary>
    /// Rate limiting middleware to prevent spam and DDoS attacks
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

     // Store request counts per IP
      private static readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();

        // Configuration
  private readonly int _maxRequestsPerMinute;
     private readonly int _maxRequestsPerSecond;
   private readonly int _banDurationMinutes;

        public RateLimitingMiddleware(
 RequestDelegate next,
 ILogger<RateLimitingMiddleware> logger,
      IConfiguration configuration)
      {
 _next = next;
  _logger = logger;
 _maxRequestsPerMinute = configuration.GetValue("RateLimiting:MaxRequestsPerMinute", 100);
         _maxRequestsPerSecond = configuration.GetValue("RateLimiting:MaxRequestsPerSecond", 10);
          _banDurationMinutes = configuration.GetValue("RateLimiting:BanDurationMinutes", 15);
     }

    public async Task InvokeAsync(HttpContext context)
     {
  var ipAddress = GetClientIpAddress(context);
      var key = $"{ipAddress}";

 // Check if IP is banned
    if (_requestCounts.TryGetValue(key, out var info) && info.IsBanned)
     {
     if (DateTime.UtcNow < info.BannedUntil)
         {
       _logger.LogWarning("Blocked request from banned IP: {IpAddress}", ipAddress);
      context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
 context.Response.Headers.Append("Retry-After", ((int)(info.BannedUntil - DateTime.UtcNow).Value.TotalSeconds).ToString());
         await context.Response.WriteAsJsonAsync(new
           {
error = "Too many requests",
      message = "You have been temporarily blocked due to excessive requests",
         retryAfter = info.BannedUntil
         });
    return;
 }
 else
  {
    // Ban expired, reset
         info.IsBanned = false;
    info.BannedUntil = null;
    lock (info.RequestTimestamps)
        {
      info.RequestTimestamps.Clear();
       }
}
          }

      // Get or create rate limit info
         var rateLimitInfo = _requestCounts.GetOrAdd(key, _ => new RateLimitInfo());

   // Check rate limits
     var now = DateTime.UtcNow;
     var oneMinuteAgo = now.AddMinutes(-1);
     var oneSecondAgo = now.AddSeconds(-1);

  int requestsInLastSecond;
    int requestsInLastMinute;
      bool shouldBlock = false;
   bool shouldBan = false;

  lock (rateLimitInfo.RequestTimestamps)
  {
    // Remove timestamps older than 1 minute
        rateLimitInfo.RequestTimestamps.RemoveAll(t => t < oneMinuteAgo);

  // Count requests
      requestsInLastSecond = rateLimitInfo.RequestTimestamps.Count(t => t > oneSecondAgo);
      requestsInLastMinute = rateLimitInfo.RequestTimestamps.Count;

   // Check rate limits
   if (requestsInLastSecond >= _maxRequestsPerSecond || requestsInLastMinute >= _maxRequestsPerMinute)
       {
        rateLimitInfo.ViolationCount++;
        shouldBlock = true;

          // Ban if too many violations
   if (rateLimitInfo.ViolationCount >= 3)
          {
  rateLimitInfo.IsBanned = true;
        rateLimitInfo.BannedUntil = now.AddMinutes(_banDurationMinutes);
        shouldBan = true;
  }
  }
            else
            {
      // Add current request timestamp
          rateLimitInfo.RequestTimestamps.Add(now);
      }
   }

            if (shouldBan)
        {
 _logger.LogWarning("IP banned for rate limit violations: {IpAddress}, Duration: {Duration}min",
       ipAddress, _banDurationMinutes);
   }

     if (shouldBlock)
            {
      _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}, Requests/sec: {PerSec}, Requests/min: {PerMin}",
       ipAddress, requestsInLastSecond, requestsInLastMinute);

 context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
         context.Response.Headers.Append("Retry-After", "60");
     context.Response.Headers.Append("X-RateLimit-Limit", _maxRequestsPerMinute.ToString());
 context.Response.Headers.Append("X-RateLimit-Remaining", "0");

              await context.Response.WriteAsJsonAsync(new
           {
           error = "Rate limit exceeded",
   message = "Too many requests. Please slow down.",
        retryAfter = 60
 });
 return;
     }

            // Add rate limit headers
       context.Response.Headers.Append("X-RateLimit-Limit", _maxRequestsPerMinute.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining",
        Math.Max(0, _maxRequestsPerMinute - requestsInLastMinute).ToString());

       await _next(context);
    }

 private static string GetClientIpAddress(HttpContext context)
   {
      // Check for forwarded headers (behind proxy/load balancer)
           var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
          if (!string.IsNullOrEmpty(forwardedFor))
   {
                return forwardedFor.Split(',')[0].Trim();
     }

  var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
    if (!string.IsNullOrEmpty(realIp))
  {
    return realIp;
  }

      return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
       }

        // Cleanup old entries periodically
     public static void CleanupOldEntries()
        {
  var keysToRemove = _requestCounts
     .Where(kvp =>
     {
 lock (kvp.Value.RequestTimestamps)
  {
   return kvp.Value.RequestTimestamps.Count == 0 &&
    (!kvp.Value.IsBanned || kvp.Value.BannedUntil < DateTime.UtcNow);
  }
   })
      .Select(kvp => kvp.Key)
      .ToList();

     foreach (var key in keysToRemove)
            {
     _requestCounts.TryRemove(key, out _);
      }
        }
    }

    public class RateLimitInfo
    {
     public List<DateTime> RequestTimestamps { get; } = new();
   public int ViolationCount { get; set; } = 0;
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedUntil { get; set; }
    }
}
