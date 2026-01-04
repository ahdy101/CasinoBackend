namespace Casino.Backend.Middleware
{
    /// <summary>
    /// Adds security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
  _next = next;
        }

    public async Task InvokeAsync(HttpContext context)
        {
    // Prevent clickjacking
      context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Prevent MIME type sniffing
         context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Enable XSS filter
   context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

  // Referrer policy
       context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy (adjust as needed)
          context.Response.Headers.Append("Content-Security-Policy",
         "default-src 'self'; " +
             "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
"style-src 'self' 'unsafe-inline'; " +
   "img-src 'self' data: https:; " +
      "font-src 'self'; " +
      "connect-src 'self'; " +
         "frame-ancestors 'none';");

          // Permissions Policy (disable unnecessary features)
            context.Response.Headers.Append("Permissions-Policy",
     "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        // Strict Transport Security (HTTPS only)
      if (context.Request.IsHttps)
         {
     context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            // Remove server header
  context.Response.Headers.Remove("Server");
      context.Response.Headers.Remove("X-Powered-By");

            await _next(context);
  }
    }
}
