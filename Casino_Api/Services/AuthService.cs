using Casino.Backend.Models;
using Casino.Backend.Services.Interfaces;
using Casino.Backend.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Casino.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _config = config;
            _logger = logger;
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            _logger.LogInformation("RegisterAsync - Username: {Username}, Email: {Email}", username, email);

            if (await _userRepository.UsernameExistsAsync(username))
            {
                _logger.LogWarning("RegisterAsync - Username already taken: {Username}", username);
                throw new Exception("Username taken");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var now = DateTime.UtcNow;
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                Balance = 10000m,
                Role = "Player",
                CreatedAt = now,
                ModifiedAt = now
            };

            await _userRepository.AddAsync(user);

            _logger.LogInformation("RegisterAsync successful - UserId: {UserId}, Username: {Username}", user.Id, username);
            return user;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            _logger.LogInformation("LoginAsync - Username: {Username}", username);

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("LoginAsync - Invalid credentials for username: {Username}", username);
                throw new Exception("Invalid credentials");
            }

            // Create JWT with role claim
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "120")),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation("LoginAsync successful - UserId: {UserId}, Role: {Role}", user.Id, user.Role);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> IsUsernameAvailable(string username)
        {
            return !await _userRepository.UsernameExistsAsync(username);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            _logger.LogInformation("ChangePasswordAsync - UserId: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ChangePasswordAsync - User not found: {UserId}", userId);
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning("ChangePasswordAsync - Current password incorrect for UserId: {UserId}", userId);
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("ChangePasswordAsync successful - UserId: {UserId}", userId);
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            _logger.LogInformation("GetUserByIdAsync - UserId: {UserId}", userId);
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("ForgotPasswordAsync - Email: {Email}", email);

            var user = await _userRepository.GetByUsernameAsync(email);
   
            // Security: Don't reveal if email exists
            if (user == null)
            {
                 _logger.LogWarning("ForgotPasswordAsync - Password reset requested for non-existent email: {Email}", email);
                return "If the email exists, a reset link has been sent";
            }

            // Invalidate any existing tokens
         await _passwordResetTokenRepository.InvalidateUserTokensAsync(user.Id);

          // Generate secure random token
            var tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
          var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");

         // Save token to database
          var resetToken = new PasswordResetToken
              {
           UserId = user.Id,
             Token = token,
         ExpiresAt = DateTime.UtcNow.AddHours(1), // Expires in 1 hour
           IsUsed = false,
          CreatedAt = DateTime.UtcNow
           };

           await _passwordResetTokenRepository.AddAsync(resetToken);

           // TODO: Send email with reset link
           // var resetLink = $"https://yoursite.com/reset-password?token={token}";
         // await _emailService.SendPasswordResetEmail(user.Email, resetLink);

        _logger.LogInformation("Password reset token generated for UserId: {UserId}", user.Id);
               return "If the email exists, a reset link has been sent";
           }

public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
      _logger.LogInformation("ResetPasswordAsync - Attempting password reset");

 // Get token from database
   var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(token);

// Validate token
     if (resetToken == null)
        {
    _logger.LogWarning("ResetPasswordAsync - Token not found");
     return false;
      }

      if (resetToken.IsUsed)
      {
      _logger.LogWarning("ResetPasswordAsync - Token already used");
    return false;
 }

   if (resetToken.ExpiresAt < DateTime.UtcNow)
     {
            _logger.LogWarning("ResetPasswordAsync - Token expired");
    return false;
        }

   // Get user and update password
    var user = await _userRepository.GetByIdAsync(resetToken.UserId);
  if (user == null)
     {
      _logger.LogWarning("ResetPasswordAsync - User not found for token");
     return false;
   }

   user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
 user.ModifiedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

  // Mark token as used
  resetToken.IsUsed = true;
   resetToken.UsedAt = DateTime.UtcNow;
       await _passwordResetTokenRepository.UpdateAsync(resetToken);

   _logger.LogInformation("Password reset successful for UserId: {UserId}", user.Id);
    return true;
        }

        public async Task<User> UpdateProfileAsync(int userId, string? newUsername, string? newEmail)
        {
            _logger.LogInformation("UpdateProfileAsync - UserId: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(newUsername) && newUsername != user.Username)
            {
                if (await _userRepository.UsernameExistsAsync(newUsername))
                    throw new Exception("Username already taken");
                user.Username = newUsername;
            }

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
            {
                if (await _userRepository.EmailExistsAsync(newEmail))
                    throw new Exception("Email already taken");
                user.Email = newEmail;
            }

            user.ModifiedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return user;
        }
    }
}
