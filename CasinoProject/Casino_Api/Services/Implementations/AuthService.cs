using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Models;
using Casino_Api.Repositories.Interfaces;
using Casino_Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Casino_Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<(bool Success, string Token, string ApiKey, UserResponse? User, string Message)> Login(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null)
            return (false, string.Empty, string.Empty, null, "Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, string.Empty, string.Empty, null, "Invalid email or password");

        var token = GenerateJwtTokenWithRole(user.Id, user.Username, user.Role);
        var apiKey = await GetDefaultApiKey();

        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Balance = user.Balance,
            CreatedAt = user.CreatedAt
        };

        return (true, token, apiKey, userResponse, "Login successful");
    }

    public async Task<(bool Success, UserResponse? User, string Message)> Register(RegisterRequest request)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            return (false, null, "Email already exists");

        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
            return (false, null, "Username already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Balance = 1000m, // Initial welcome bonus
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Balance = user.Balance,
            CreatedAt = user.CreatedAt
        };

        return (true, userResponse, "Registration successful");
    }

    public async Task<(bool Success, UserResponse? User, string Message)> GetUserById(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            return (false, null, "User not found");

        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Balance = user.Balance,
            CreatedAt = user.CreatedAt
        };

        return (true, userResponse, "Success");
    }

    public async Task<(bool Success, string Message)> UpdateBalance(int userId, decimal newBalance)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            return (false, "User not found");

        user.Balance = newBalance;
        await _unitOfWork.SaveChangesAsync();

        return (true, "Balance updated successfully");
    }

    public string GenerateJwtToken(int userId, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "120")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GetDefaultApiKey()
    {
        var apiKey = await _unitOfWork.TenantApiKeys.GetActiveApiKeyAsync();
        return apiKey?.ApiKey ?? "default_tenant_api_key_12345";
    }

    public async Task<(bool Success, string Token, UserResponse? User, string ErrorCode, string Message)> AuthenticateWithToken(TokenRequest request)
    {
        // Validate grant_type
        if (request.GrantType?.ToLower() != "password")
        {
            return (false, string.Empty, null, "unsupported_grant_type", "Only 'password' grant type is supported");
        }

        // Validate WebAPI Key
        var isValidApiKey = await _unitOfWork.TenantApiKeys.ValidateApiKeyAsync(request.WebApiKey);
        if (!isValidApiKey)
        {
            return (false, string.Empty, null, "invalid_client", "Invalid webapi_key");
        }

        // Authenticate user by username or email
        User? user = null;
        
        // Try to find by email first
        if (request.Username.Contains("@"))
        {
            user = await _unitOfWork.Users.GetByEmailAsync(request.Username);
        }
        else
        {
            // Try by username
            user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        }

        if (user == null)
        {
            return (false, string.Empty, null, "invalid_grant", "Invalid username or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return (false, string.Empty, null, "invalid_grant", "Invalid username or password");
        }

        // Generate token with 30 minutes expiration and role
        var token = GenerateJwtTokenWithRoleAndExpiry(user.Id, user.Username, user.Role, 30);

      var userResponse = new UserResponse
        {
            Id = user.Id,
   Username = user.Username,
   Email = user.Email,
          Balance = user.Balance,
  CreatedAt = user.CreatedAt
        };

  return (true, token, userResponse, string.Empty, "Authentication successful");
    }

    private string GenerateJwtTokenWithRole(int userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
   {
  new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
       new Claim(ClaimTypes.Name, username),
       new Claim(ClaimTypes.Role, role),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
         audience: _config["Jwt:Audience"],
          claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "120")),
    signingCredentials: credentials
        );

  return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateJwtTokenWithRoleAndExpiry(int userId, string username, string role, int expiryMinutes)
    {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
     new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
       new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
   audience: _config["Jwt:Audience"],
            claims: claims,
         expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
   signingCredentials: credentials
 );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
