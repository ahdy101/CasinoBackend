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
