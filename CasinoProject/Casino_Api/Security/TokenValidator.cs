using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Casino_Api.Security;

/// <summary>
/// Helper class for JWT token validation
/// </summary>
public class TokenValidator
{
    private readonly IConfiguration _configuration;

 public TokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Validates bearer token and returns user ID if valid
    /// </summary>
    public (bool IsValid, int UserId, string Username, string Role, string Error) ValidateToken(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
      return (false, 0, string.Empty, string.Empty, "Authorization header is missing");

  if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
       return (false, 0, string.Empty, string.Empty, "Invalid authorization header format. Use: Bearer {token}");

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(token))
            return (false, 0, string.Empty, string.Empty, "Token is missing");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "");

            var validationParameters = new TokenValidationParameters
       {
          ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(key),
 ValidateIssuer = true,
  ValidIssuer = _configuration["Jwt:Issuer"],
    ValidateAudience = true,
        ValidAudience = _configuration["Jwt:Audience"],
          ValidateLifetime = true,
  ClockSkew = TimeSpan.Zero
};

       var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
    var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value ?? "User";

    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
     return (false, 0, string.Empty, string.Empty, "Invalid token claims");

        return (true, userId, usernameClaim ?? "", roleClaim, string.Empty);
        }
        catch (SecurityTokenExpiredException)
        {
         return (false, 0, string.Empty, string.Empty, "Token has expired");
        }
        catch (Exception ex)
        {
          return (false, 0, string.Empty, string.Empty, $"Token validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates token and checks if user has required role
    /// </summary>
    public (bool IsValid, int UserId, string Username, string Error) ValidateTokenWithRole(string? authorizationHeader, params string[] requiredRoles)
    {
        var result = ValidateToken(authorizationHeader);
        
     if (!result.IsValid)
    return (false, 0, string.Empty, result.Error);

 if (requiredRoles.Length > 0 && !requiredRoles.Contains(result.Role))
        return (false, 0, string.Empty, $"Access denied. Required role: {string.Join(" or ", requiredRoles)}");

   return (true, result.UserId, result.Username, string.Empty);
  }
}
