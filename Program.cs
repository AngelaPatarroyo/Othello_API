using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using Othello_API.Interfaces;
using Othello_API.Services;
using Othello_API.Repositories;
using Microsoft.OpenApi.Models;
using System.Text;

Env.Load(); // Load .env file

var builder = WebApplication.CreateBuilder(args);

// Load Configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .Build();

// Database Connection
var dbConnection = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                   ?? "Data Source=/app/OthelloDB.sqlite"; //  Fallback to default if env var is missing


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(dbConnection));

// Identity & Authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? config["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new InvalidOperationException("JWT Secret is missing.");

byte[] key = Encoding.UTF8.GetBytes(jwtSecret);
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

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole("Player"));
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("https://your-frontend-domain.com", "http://localhost:3000")
                        .AllowAnyMethod()
                        .WithHeaders("Authorization", "Content-Type"));
});

// Add Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,  // Max 100 requests
            Period = "1m" // Per minute
        }
    };
});
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

// JSON Configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Services & Repositories
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();
builder.Services.AddScoped<ILeaderBoardService, LeaderBoardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGameService, UserGameService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Othello_API", Version = "1.0" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer YOUR_TOKEN_HERE'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

builder.Services.AddScoped<EmailService>();

// Build & Run Application
var app = builder.Build();

// Apply Pending Migrations Automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.Migrate(); // Ensure database is up to date

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Admin", "Player" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"Role '{roleName}' created.");
        }
    }
}

// Apply Security Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Othello_API.Middleware.ErrorHandlingMiddleware>();
app.MapControllers();
app.Run();
