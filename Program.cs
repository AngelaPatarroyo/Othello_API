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
    .AddEnvironmentVariables() // Load Environment Variables FIRST
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

// Handle JWT Secret Key properly
byte[] key;
if (jwtSecret.Length >= 44 && jwtSecret.EndsWith("="))
{
    try
    {
        key = Convert.FromBase64String(jwtSecret);
        Console.WriteLine($"Decoded Base64 JWT Secret. Key length: {key.Length} bytes.");
    }
    catch (FormatException)
    {
        Console.WriteLine("JWT Secret was expected to be Base64 but failed decoding. Using as plain text.");
        key = Encoding.UTF8.GetBytes(jwtSecret);
    }
}
else
{
    Console.WriteLine("Using plain text JWT Secret.");
    key = Encoding.UTF8.GetBytes(jwtSecret);
}

if (key.Length < 32)
{
    throw new InvalidOperationException($"JWT Secret key must be at least 32 bytes, but found {key.Length} bytes.");
}

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Othello_API", Version = "1.0" });

    // 🔹Add JWT authentication to Swagger
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

// Build the application
var app = builder.Build();

// Apply Pending Migrations Automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.Migrate(); //  Ensure database is up to date

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
