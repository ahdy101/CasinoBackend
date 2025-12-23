using Casino.Backend.Data;
using Casino.Backend.Infrastructure;
using Casino.Backend.Services;
using Casino.Backend.Services.Implementations;
using Casino.Backend.Services.Interfaces;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

// Register Dapper Database Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register Dapper Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBetRepository, BetRepository>();
builder.Services.AddScoped<IBlackjackGameRepository, BlackjackGameRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<ITenantApiKeyRepository, TenantApiKeyRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

// JWT Auth - Validate configuration
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Please ensure 'Jwt:Key' is set in appsettings.json with a value of at least 32 characters.");
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
     $"JWT Key must be at least 32 characters long. Current length: {jwtKey.Length}");
}

var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey);

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

// Add CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
 {
        policy.WithOrigins(
    "http://localhost:3000",// React default
            "http://localhost:5173",   // Vite default
    "http://localhost:4200",   // Angular default
   "http://localhost:8080"    // Vue default
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers()
  .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // API Documentation
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Casino Backend API",
 Version = "v1",
        Description = "Casino gaming platform with Blackjack, Roulette, and wallet management. Auth endpoints are public. Game endpoints require JWT Bearer token + API key.",
     Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
    Name = "Casino Backend Support",
            Email = "support@casino.com"
        }
    });

    // JWT Bearer Authentication for Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Get your token from POST /api/auth/login, then enter: Bearer {your_token}",
     Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
     Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT"
});

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
     {
          Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
           Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
 Id = "Bearer"
    }
         },
   Array.Empty<string>()
        }
    });

    // Clean schema IDs to avoid conflicts
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    
    // Support nullable reference types
  options.SupportNonNullableReferenceTypes();
    
    // Use data annotations for descriptions
    options.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// Enable CORS before authentication
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
