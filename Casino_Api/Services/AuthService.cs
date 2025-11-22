using Casino.Backend.Data;
using Casino.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Casino.Backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

        public async Task<User> RegisterAsync(string username, string password)
        {
            if (await _db.Users.AnyAsync(u => u.Username == username)) throw new Exception("Username taken");
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, PasswordHash = hash, Balance = 1000m }; // seed
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) throw new Exception("Invalid credentials");

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
            return tokenHandler.WriteToken(token);
        }
    }
}
