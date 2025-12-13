using Casino.Backend.Data;
using Casino.Backend.Infrastructure;
using Casino.Backend.Services;
using Casino.Backend.Services.Implementations;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

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

// Register Services - Service Layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IBlackjackEngine, BlackjackEngine>();
builder.Services.AddScoped<IRouletteEngine, RouletteEngine>();

// Register Infrastructure Services
builder.Services.AddSingleton<IRandomNumberGenerator, CryptoRNG>();
builder.Services.AddSingleton<ICardDeckFactory, CardDeckFactory>();

builder.Services.AddControllers()
  .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
