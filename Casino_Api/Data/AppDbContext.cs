using Casino.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Casino.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<TenantApiKey> TenantApiKeys { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<BlackjackGame> BlackjackGames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<TenantApiKey>().HasIndex(t => t.ApiKey).IsUnique();
            modelBuilder.Entity<AdminUser>().HasIndex(a => a.Username).IsUnique();

            // Relationships
            modelBuilder.Entity<BlackjackGame>()
         .HasOne(bg => bg.User)
       .WithMany()
          .HasForeignKey(bg => bg.UserId)
           .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
