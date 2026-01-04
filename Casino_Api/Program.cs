using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Implementations;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services;
using Casino.Backend.Services.Interfaces;
using Casino.Backend.Services.Implementations;
using Casino.Backend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

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
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
        "http://localhost:3000",   // React default
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
 // Clean schema IDs to avoid conflicts
options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    
    // Support nullable reference types
    options.SupportNonNullableReferenceTypes();
    
    // Use data annotations for descriptions
    options.UseInlineDefinitionsForEnums();
    
    // Enable annotations support
    options.EnableAnnotations();

    // Add Bearer token authorization using Swashbuckle's built-in support
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.\n\nExample: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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
});

var app = builder.Build();

// Enable CORS before authentication
app.UseCors("AllowFrontend");

// Serve static files (for swagger-auth-inject.js)
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Casino API v1");
});

app.UseAuthentication();
app.UseAuthorization();

// Track user activity on every authenticated request
app.UseMiddleware<ActivityTrackingMiddleware>();

app.MapControllers();
app.Run();
