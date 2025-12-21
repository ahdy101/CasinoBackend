using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;

namespace Casino_Api.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Token, string ApiKey, UserResponse? User, string Message)> Login(LoginRequest request);
    Task<(bool Success, UserResponse? User, string Message)> Register(RegisterRequest request);
    Task<(bool Success, UserResponse? User, string Message)> GetUserById(int userId);
    Task<(bool Success, string Message)> UpdateBalance(int userId, decimal newBalance);
    string GenerateJwtToken(int userId, string username);
    Task<(bool Success, string Token, UserResponse? User, string ErrorCode, string Message)> AuthenticateWithToken(TokenRequest request);
}
