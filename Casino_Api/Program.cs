using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Implementations;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services;
using Casino.Backend.Services.Interfaces;
using Casino.Backend.Services.Implementations;
using Casino.Backend.Middleware;
using Casino.Backend.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURATION: Environment Variables Override
// ============================================
// Environment variables override appsettings.json values
// Format: Section__Key (double underscore for nested keys)
// Example: Jwt__Key, Stripe__SecretKey, ConnectionStrings__DefaultConnection

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Helper to get config with environment variable fallback
string GetConfigValue(string key, string? envVarName = null)
{
    // Try environment variable first
    if (envVarName != null)
    {
      var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
      return envValue;
    }
    
    // Fall back to configuration
    return builder.Configuration[key] ?? throw new InvalidOperationException($"Configuration '{key}' is not set. Set environment variable '{envVarName ?? key}' or update appsettings.");
}

// ============================================
// SECURITY: Request size limits
// ============================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB max
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32 KB headers
    options.Limits.MaxRequestLineSize = 8 * 1024; // 8 KB request line
});

// ============================================
// SECURITY: Built-in Rate Limiting (.NET 7+)
// ============================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
     {
      PermitLimit = 100,
     Window = TimeSpan.FromMinutes(1),
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
      QueueLimit = 5
        });
    });

    // Stricter limit for auth endpoints (prevent brute force)
    options.AddPolicy("auth", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
    PermitLimit = 10,
      Window = TimeSpan.FromMinutes(1),
      QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
     QueueLimit = 2
 });
    });

    // Rate limit for game actions
    options.AddPolicy("game", context =>
 {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
{
     PermitLimit = 60,
     Window = TimeSpan.FromMinutes(1),
 SegmentsPerWindow = 6,
       QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 10
 });
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
   error = "Too many requests",
  message = "Rate limit exceeded. Please try again later.",
    retryAfter = 60
}, token);
    };
});

// Register Dapper Database Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register Dapper Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBetRepository, BetRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<ITenantApiKeyRepository, TenantApiKeyRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// ============================================
// JWT Authentication
// ============================================
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("${"))
{
    if (builder.Environment.IsDevelopment())
    {
        // Use a default dev key (not for production!)
 jwtKey = "DevOnlySecretKey-NotForProduction-MustBeAtLeast32Characters!@#$%2024";
        Console.WriteLine("WARNING: Using default development JWT key. Set JWT_SECRET_KEY for production!");
    }
    else
    {
        throw new InvalidOperationException(
       "JWT_SECRET_KEY environment variable is not configured for production.");
    }
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        $"JWT Key must be at least 32 characters long. Current length: {jwtKey.Length}");
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
?? builder.Configuration["Jwt:Issuer"] 
    ?? "CasinoBackend";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? builder.Configuration["Jwt:Audience"] 
    ?? "CasinoUsers";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
     IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

// Register Services - Service Layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ============================================
// CORS Configuration
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Try environment variable first
     var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        string[] allowedOrigins;
        
        if (!string.IsNullOrEmpty(corsOrigins))
        {
    allowedOrigins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
   }
        else
        {
  allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000", "http://localhost:5173", "http://localhost:4200", "http://localhost:8080"];
        }
  
      policy.WithOrigins(allowedOrigins)
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
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    options.SupportNonNullableReferenceTypes();
    options.UseInlineDefinitionsForEnums();
    options.EnableAnnotations();
    options.OperationFilter<AuthorizationHeaderOperationFilter>();
});

// ============================================
// SECURITY: Add HSTS
// ============================================
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
  options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// ============================================
// Log configuration on startup (dev only)
// ============================================
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("=== Casino API Configuration ===");
    Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
  Console.WriteLine($"JWT Issuer: {jwtIssuer}");
    Console.WriteLine($"JWT Audience: {jwtAudience}");
    Console.WriteLine($"JWT Key Length: {jwtKey.Length} chars");
  Console.WriteLine("================================");
}

// ============================================
// MIDDLEWARE ORDER IS CRITICAL FOR SECURITY
// ============================================

// 1. Security headers (first to apply to all responses)
app.UseMiddleware<SecurityHeadersMiddleware>();

// 2. Request validation (block malicious requests early)
app.UseMiddleware<RequestValidationMiddleware>();

// 3. Rate limiting (prevent spam/DDoS)
app.UseRateLimiter();

// 4. HTTPS redirection (production)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// 5. CORS
app.UseCors("AllowFrontend");

// 6. Static files
app.UseStaticFiles();

// 7. Swagger (consider disabling in production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Casino API v1");
  });
}

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Activity tracking
app.UseMiddleware<ActivityTrackingMiddleware>();

// 10. Map controllers
app.MapControllers();

app.Run();
