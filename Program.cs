using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Othello_API.Interfaces;
using Othello_API.Services;
using Othello_API.Repositories;
using Microsoft.OpenApi.Models;
using System.Text;

Env.Load(); // Load .env file

// Build Configuration
var builder = WebApplication.CreateBuilder(args);

// Explicitly load configuration (Environment Variables FIRST)
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables() // 🔹 Load Environment Variables FIRST
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .Build();

// Debugging: Print Environment Variables
Console.WriteLine($"DATABASE_URL from env: {Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")}");
Console.WriteLine($"JWT_SECRET from env: {Environment.GetEnvironmentVariable("JWT_SECRET")}");

// Ensure the database connection is set
var dbConnection = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(dbConnection))
{
    throw new InvalidOperationException("Database connection string is missing. Set it as an environment variable.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(dbConnection));

// Register Identity for authentication & role management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Ensure JWT Secret is set
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? config["JwtSettings:Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is missing. Set it as an environment variable or in GitHub Secrets.");
}

var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole("Player"));
});

// Secure CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("https://your-frontend-domain.com", "http://localhost:3000") 
                        .AllowAnyMethod()
                        .WithHeaders("Authorization", "Content-Type"));
});

// Add JSON options to handle object cycles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Register services and repositories (Dependency Injection)
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();
builder.Services.AddScoped<ILeaderBoardService, LeaderBoardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGameService, UserGameService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();

// Add API documentation (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<EmailService>();

// Build the application
var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
