using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Casino.Backend.Attributes
{
    /// <summary>
    /// Adds Authorization header parameter to Swagger documentation
    /// </summary>
    public class SwaggerAuthorizationAttribute : SwaggerOperationAttribute
    {
        public SwaggerAuthorizationAttribute() : base()
        {
        }
    }
    
    /// <summary>
    /// Custom parameter attribute for Authorization header
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerAuthHeaderAttribute : Attribute
    {
        public string Name => "Authorization";
        public string Description => "JWT Bearer token (e.g., Bearer eyJhbGc...)";
        public bool Required => false;
    }
}
