using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Casino.Backend.Data;
using System.Linq;

namespace Casino.Backend.Security
{
 public class RequireApiKeyAttribute : Attribute, IAsyncActionFilter
 {
 public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
 {
 var httpContext = context.HttpContext;
 var apiKeyHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
 if (apiKeyHeader != null && apiKeyHeader.StartsWith("Bearer "))
 {
 var apiKey = apiKeyHeader.Substring("Bearer ".Length).Trim();
 var db = httpContext.RequestServices.GetRequiredService<AppDbContext>();
 var valid = db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
 if (valid)
 {
 await next();
 return;
 }
 }
 context.Result = new ContentResult
 {
 StatusCode =401,
 Content = "Invalid or missing API key."
 };
 }
 }
}
