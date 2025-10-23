using KNOTS.Models;
using Microsoft.EntityFrameworkCore;
using KNOTS.Services;

namespace KNOTS.Data;
public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<GameStatement> Statements { get; set; }
    public DbSet<PlayerSwipeRecord> PlayerSwipes { get; set; }
    public DbSet<GameHistoryRecord> GameHistory { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username); 
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.TotalGamesPlayed).HasDefaultValue(0);
            entity.Property(e => e.BestMatchesCount).HasDefaultValue(0);
            entity.Property(e => e.AverageCompatibilityScore).HasDefaultValue(0.0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });
        // GameStatement configuration
        modelBuilder.Entity<GameStatement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
        });
        // PlayerSwipeRecord configuration
        modelBuilder.Entity<PlayerSwipeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PlayerUsername).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StatementId).IsRequired().HasMaxLength(10);
            entity.Property(e => e.StatementText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SwipedAt).HasDefaultValueSql("datetime('now')");

            entity.HasIndex(e => new { e.RoomCode, e.PlayerUsername, e.StatementId })
                .IsUnique();
        });
        // GameHistoryRecord configuration
        modelBuilder.Entity<GameHistoryRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PlayedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.PlayerUsernames).IsRequired();
            entity.Property(e => e.BestMatchPlayer).HasMaxLength(50);
            entity.Property(e => e.ResultsJson).IsRequired();
        });
    }
}

