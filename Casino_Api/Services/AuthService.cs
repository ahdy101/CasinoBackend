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
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
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
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                Balance = 10000m,
                Role = "Player",
                CreatedAt = DateTime.UtcNow
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

            // Create JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username) }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "120")),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation("LoginAsync successful - UserId: {UserId}", user.Id);
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
    }
}
