using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;
using Othello_API.Interfaces;
using Othello_API.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Othello_API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add logging to the console
builder.Logging.AddConsole();

// Add database connection (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity for authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add JSON options to handle object cycles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names as defined
        options.JsonSerializerOptions.WriteIndented = true; // Pretty JSON formatting
    });


// Register services and repositories (Dependency Injection)
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMoveService, MoveService>();
builder.Services.AddScoped<ILeaderBoardService, LeaderBoardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGameService, UserGameService>(); // Added UserGame service

builder.Services.AddScoped<IUserRepository, UserRepository>();  
builder.Services.AddScoped<IGameRepository, GameRepository>(); 
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>(); // Added UserGame repository

// Add API exploration and Swagger for documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Othello API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
