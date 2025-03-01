using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Othello_API.Models;
using Othello_API.Interfaces;
using Othello_API.Services;
using Othello_API.Repositories;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

// Build Configuration
var builder = WebApplication.CreateBuilder(args);

// Explicitly load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Add logging to the console
builder.Logging.AddConsole();

// Add database connection (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(config.GetConnectionString("DefaultConnection")));

// Register Identity for authentication & role management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var secret = config.GetValue<string>("JwtSettings:Secret") ?? throw new Exception("JWT Secret is missing!");
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole("Player"));
});

// Enable CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
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

// Add API exploration and Swagger for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Othello API", Version = "v1" });
});

// Build the application
var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Enable CORS
app.UseAuthentication(); // Enable JWT Authentication
app.UseAuthorization();  // Enable Authorization Middleware

app.MapControllers();
app.Run();
foreach (var kvp in config.AsEnumerable())
{
    Console.WriteLine($"Config Key: {kvp.Key} - Value: {kvp.Value}");
}
