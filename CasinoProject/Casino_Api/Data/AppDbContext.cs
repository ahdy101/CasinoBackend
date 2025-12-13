using Casino_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Entity Sets
    public DbSet<User> Users { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<Bet> Bets { get; set; }
    public DbSet<TenantApiKey> TenantApiKeys { get; set; }
    public DbSet<BlackjackGame> BlackjackGames { get; set; }
    public DbSet<PokerTable> PokerTables { get; set; }
    public DbSet<GameHistory> GameHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<TenantApiKey>()
            .HasIndex(t => t.ApiKey)
            .IsUnique();

        // Relationships
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bets)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BlackjackGame>()
            .HasOne(b => b.User)
            .WithMany(u => u.BlackjackGames)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameHistory>()
            .HasOne(g => g.User)
            .WithMany(u => u.GameHistories)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision
        modelBuilder.Entity<User>()
            .Property(u => u.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Bet>()
            .Property(b => b.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Bet>()
            .Property(b => b.Payout)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BlackjackGame>()
            .Property(b => b.BetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BlackjackGame>()
            .Property(b => b.Payout)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PokerTable>()
            .Property(p => p.BuyIn)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PokerTable>()
            .Property(p => p.Pot)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GameHistory>()
            .Property(g => g.BetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GameHistory>()
            .Property(g => g.Payout)
            .HasPrecision(18, 2);

        // Seed data
        SeedAdminUsers(modelBuilder);
        SeedTenantApiKeys(modelBuilder);
    }

    private void SeedAdminUsers(ModelBuilder modelBuilder)
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        modelBuilder.Entity<AdminUser>().HasData(new AdminUser
        {
            Id = 1,
            Username = "admin",
            PasswordHash = adminHash,
            Role = "SuperAdmin",
            CreatedAt = DateTime.UtcNow
        });
    }

    private void SeedTenantApiKeys(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantApiKey>().HasData(new TenantApiKey
        {
            Id = 1,
            TenantName = "DefaultTenant",
            ApiKey = "default_tenant_api_key_12345",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
