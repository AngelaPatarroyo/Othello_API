using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Game> Games { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<LeaderBoard> LeaderBoard { get; set; }
    public DbSet<Move> Moves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One-to-One: User ↔ Leaderboard
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.LeaderBoard)
            .WithOne(lb => lb.Player)
            .HasForeignKey<LeaderBoard>(lb => lb.PlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-Many: User ↔ Game via UserGame
        modelBuilder.Entity<UserGame>()
            .HasKey(ug => ug.UserGameId);

        modelBuilder.Entity<UserGame>()
    .HasOne<ApplicationUser>(ug => ug.User)
    .WithMany(u => u.UserGames)
    .HasForeignKey(ug => ug.UserId)
    .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<UserGame>()
            .HasOne(ug => ug.Game)
            .WithMany(g => g.UserGames)
            .HasForeignKey(ug => ug.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Game → Player1 (required)
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Player1)
            .WithMany()
            .HasForeignKey(g => g.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Game → Player2 (required)
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Player2)
            .WithMany()
            .HasForeignKey(g => g.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Game → Winner (optional)
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Winner)
            .WithMany()
            .HasForeignKey(g => g.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
