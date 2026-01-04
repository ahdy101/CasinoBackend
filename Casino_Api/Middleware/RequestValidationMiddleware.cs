using System.Text.RegularExpressions;

namespace Casino.Backend.Middleware
{
    /// <summary>
    /// Validates incoming requests for suspicious patterns
    /// </summary>
    public class RequestValidationMiddleware
    {
      private readonly RequestDelegate _next;
     private readonly ILogger<RequestValidationMiddleware> _logger;

   // Suspicious patterns to block
        private static readonly Regex SqlInjectionPattern = new(
       @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|EXEC|EXECUTE)\b.*\b(FROM|INTO|WHERE|TABLE)\b)|('.*(--))|(\b(OR|AND)\b\s+\d+\s*=\s*\d+)",
       RegexOptions.IgnoreCase | RegexOptions.Compiled);
      
 private static readonly Regex XssPattern = new(
      @"<script|javascript:|on\w+\s*=|<iframe|<object|<embed|<svg|<img\s+[^>]*onerror",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Blocked user agents (bots, scanners)
        private static readonly string[] BlockedUserAgents =
        [
 "sqlmap", "nikto", "nmap", "masscan", "zgrab", "gobuster",
         "dirbuster", "wpscan", "nuclei", "httpx", "subfinder"
    ];

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
  {
            _next = next;
         _logger = logger;
        }

     public async Task InvokeAsync(HttpContext context)
      {
var userAgent = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
            var path = context.Request.Path.ToString();
       var query = context.Request.QueryString.ToString();

 // Block suspicious user agents
  if (BlockedUserAgents.Any(blocked => userAgent.Contains(blocked)))
           {
        _logger.LogWarning("Blocked suspicious user agent: {UserAgent}, IP: {IP}",
     userAgent, context.Connection.RemoteIpAddress);
     context.Response.StatusCode = 403;
  await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    return;
         }

      // Check for SQL injection attempts
     if (SqlInjectionPattern.IsMatch(path) || SqlInjectionPattern.IsMatch(query))
     {
     _logger.LogWarning("Blocked SQL injection attempt. Path: {Path}, Query: {Query}, IP: {IP}",
        path, query, context.Connection.RemoteIpAddress);
      context.Response.StatusCode = 400;
   await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
         return;
    }

            // Check for XSS attempts
        if (XssPattern.IsMatch(path) || XssPattern.IsMatch(query))
      {
 _logger.LogWarning("Blocked XSS attempt. Path: {Path}, Query: {Query}, IP: {IP}",
       path, query, context.Connection.RemoteIpAddress);
     context.Response.StatusCode = 400;
    await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
       return;
      }

            // Block path traversal attempts
   if (path.Contains("..") || path.Contains("//"))
   {
   _logger.LogWarning("Blocked path traversal attempt. Path: {Path}, IP: {IP}",
    path, context.Connection.RemoteIpAddress);
       context.Response.StatusCode = 400;
     await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
   return;
        }

            // Block common scanner paths
  string[] suspiciousPaths =
       [
 "/wp-admin", "/wp-login", "/phpmyadmin", "/admin.php",
  "/.env", "/.git", "/config.php", "/web.config",
      "/.well-known/security.txt", "/actuator", "/console"
   ];

      if (suspiciousPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
   _logger.LogWarning("Blocked scanner probe. Path: {Path}, IP: {IP}",
     path, context.Connection.RemoteIpAddress);
         context.Response.StatusCode = 404;
     return;
    }

      await _next(context);
      }
    }
}
