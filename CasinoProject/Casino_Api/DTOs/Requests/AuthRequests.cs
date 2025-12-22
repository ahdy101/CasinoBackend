using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Casino_Api.DTOs.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Initial balance must be a positive number")]
    public decimal InitialBalance { get; set; } = 10000;
}

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateBalanceRequest
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Balance must be a positive number")]
    public decimal Balance { get; set; }
}

/// <summary>
/// OAuth2-style token request with grant_type parameter
/// Supports both camelCase and PascalCase JSON
/// </summary>
public class TokenRequest
{
    [Required(ErrorMessage = "grant_type is required")]
  [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "username is required")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "password is required")]
    [JsonPropertyName("password")]
  public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "webapi_key is required")]
    [JsonPropertyName("webapi_key")]
    public string WebApiKey { get; set; } = string.Empty;
}
