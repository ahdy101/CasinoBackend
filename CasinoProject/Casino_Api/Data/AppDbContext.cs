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
    public DbSet<GameState> GameStates { get; set; }
    public DbSet<GameStatistics> GameStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints for Users
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique constraints for AdminUsers
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
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

        // GameState relationships
        modelBuilder.Entity<GameState>()
            .HasOne(gs => gs.User)
            .WithMany()
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameState>()
            .HasIndex(gs => new { gs.UserId, gs.GameType })
            .IsUnique();

        // GameStatistics relationships
        modelBuilder.Entity<GameStatistics>()
            .HasOne(gs => gs.User)
            .WithMany()
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameStatistics>()
            .HasIndex(gs => new { gs.UserId, gs.GameType })
            .IsUnique();

        modelBuilder.Entity<GameStatistics>()
            .Property(gs => gs.TotalWagered)
            .HasPrecision(18, 2);

        modelBuilder.Entity<GameStatistics>()
            .Property(gs => gs.TotalWon)
            .HasPrecision(18, 2);

        // Seed data
        SeedAdminUsers(modelBuilder);
        SeedTenantApiKeys(modelBuilder);
    }

    private void SeedAdminUsers(ModelBuilder modelBuilder)
    {
        // Seed an admin user in the Users table for login
        var adminUserHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 999,
            Username = "admin",
            Email = "admin@casinoapi.com",
            PasswordHash = adminUserHash,
            Balance = 999999m,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
