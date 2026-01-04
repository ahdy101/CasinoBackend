using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Casino.Backend.Filters;

/// <summary>
/// Swagger operation filter that adds an Authorization header parameter to all endpoints
/// except login and register. Shows different descriptions for admin vs user endpoints.
/// </summary>
public class AuthorizationHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has [AllowAnonymous] attribute
        var allowAnonymous = context.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null
            || context.MethodInfo.DeclaringType?.GetCustomAttribute<AllowAnonymousAttribute>() != null;

        if (allowAnonymous)
        {
         return; // Don't add Authorization parameter to anonymous endpoints
     }

     // Skip login and register endpoints
        var methodName = context.MethodInfo.Name.ToLowerInvariant();
        var routePath = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? "";

        if (methodName.Contains("login") || methodName.Contains("register") ||
            routePath.Contains("login") || routePath.Contains("register"))
        {
      return;
        }

  // Check if this is an admin-only endpoint
        var authorizeAttribute = context.MethodInfo.GetCustomAttribute<AuthorizeAttribute>()
   ?? context.MethodInfo.DeclaringType?.GetCustomAttribute<AuthorizeAttribute>();

    var isAdminEndpoint = authorizeAttribute?.Roles?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true
 || routePath.Contains("admin");

        // Initialize parameters list if null
  operation.Parameters ??= new List<OpenApiParameter>();

 // Add Authorization header parameter with appropriate description
        var description = isAdminEndpoint
        ? "**Admin JWT Authorization token required.**\n\nOnly users with Admin role can access this endpoint.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            : "JWT Authorization token. Enter your full Bearer token here.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        operation.Parameters.Add(new OpenApiParameter
        {
      Name = "Authorization",
        In = ParameterLocation.Header,
      Description = description,
     Required = isAdminEndpoint, // Admin endpoints require authorization
            Schema = new OpenApiSchema
   {
     Type = "string"
}
    });

        // Add a note to the operation summary for admin endpoints
        if (isAdminEndpoint)
        {
      operation.Summary = $"[ADMIN ONLY] {operation.Summary}";
        }
    }
}
