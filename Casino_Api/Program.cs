using Casino.Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DB context
builder.Services.AddDbContext<AppDbContext>(options =>
 options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey ?? string.Empty);

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = builder.Configuration["Jwt:Issuer"],
 ValidAudience = builder.Configuration["Jwt:Audience"],
 IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
 };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
