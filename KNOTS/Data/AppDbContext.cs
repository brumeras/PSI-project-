using Microsoft.EntityFrameworkCore;
using KNOTS.Services;

namespace KNOTS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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
        }
    }
}