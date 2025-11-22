using Microsoft.EntityFrameworkCore;
using Casino_Admin.Models;

namespace Casino_Admin.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<AdminUser>().HasIndex(a => a.Username).IsUnique();
            // Seed an admin user
            var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            modelBuilder.Entity<User>().HasData(new User {
                Id = 1,
                Username = "admin",
                PasswordHash = adminHash,
                Balance = 0m,
                CreatedAt = DateTime.UtcNow,
                Role = "Admin"
            });
        }
    }
}