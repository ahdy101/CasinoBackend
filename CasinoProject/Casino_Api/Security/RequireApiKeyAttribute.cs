using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Casino_Api.Repositories.Interfaces;

namespace Casino_Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var apiKey = context.HttpContext.Request.Query["apiKey"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "API key is required" });
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var isValid = await unitOfWork.TenantApiKeys.ValidateApiKeyAsync(apiKey);
        if (!isValid)
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Invalid API key" });
        }
    }
}
