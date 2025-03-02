using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets for your models
    public DbSet<Game> Games { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<LeaderBoard> LeaderBoard { get; set; }
    public DbSet<Move> Moves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  Ensure Identity tables are configured
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.LeaderBoard)
            .WithOne(lb => lb.Player)
            .HasForeignKey<LeaderBoard>(lb => lb.PlayerId)
            .OnDelete(DeleteBehavior.Cascade); //  Deleting a User removes their LeaderBoard entry

        //  Many-to-Many Relationship: User <-> Game via UserGame
        modelBuilder.Entity<UserGame>()
            .HasKey(ug => ug.UserGameId); // Primary Key

        modelBuilder.Entity<UserGame>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGames)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade); //  Deleting a User removes related UserGames

        modelBuilder.Entity<UserGame>()
            .HasOne(ug => ug.Game)
            .WithMany(g => g.UserGames)
            .HasForeignKey(ug => ug.GameId)
            .OnDelete(DeleteBehavior.Cascade); //  Deleting a Game removes related UserGames

        //  Game relationships (Prevent accidental user deletion)
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Player1)
            .WithMany()
            .HasForeignKey(g => g.Player1Id)
            .OnDelete(DeleteBehavior.Restrict); // 🔹 Prevent accidental deletion of Player1

        modelBuilder.Entity<Game>()
            .HasOne(g => g.Player2)
            .WithMany()
            .HasForeignKey(g => g.Player2Id)
            .OnDelete(DeleteBehavior.Restrict); // 🔹 Prevent accidental deletion of Player2

        //  Prevent EF Core from auto-including navigation properties in UserGame
        modelBuilder.Entity<UserGame>()
            .Ignore(ug => ug.User)
            .Ignore(ug => ug.Game);
    }
}
