namespace Casino_Api.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
}

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// OAuth2-style token response
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; } // seconds
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = null!;
}

/// <summary>
/// OAuth2 error response
/// </summary>
public class TokenErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
}
