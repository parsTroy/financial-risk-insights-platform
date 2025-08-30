using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Data;

public class FinancialRiskDbContext : DbContext
{
    public FinancialRiskDbContext(DbContextOptions<FinancialRiskDbContext> options) : base(options)
    {
    }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioHolding> PortfolioHoldings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Asset entity with lowercase table and column names
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.Symbol).HasColumnName("symbol").IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sector).HasColumnName("sector").HasMaxLength(50);
            entity.Property(e => e.Industry).HasColumnName("industry").HasMaxLength(50);
            entity.Property(e => e.AssetType).HasColumnName("assettype").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            
            // Optimize for PostgreSQL
            entity.HasIndex(e => e.Symbol).IsUnique().HasDatabaseName("ix_assets_symbol");
            entity.HasIndex(e => e.Sector).HasDatabaseName("ix_assets_sector");
            entity.HasIndex(e => e.AssetType).HasDatabaseName("ix_assets_assettype");
        });

        // Configure Price entity with lowercase table and column names
        modelBuilder.Entity<Price>(entity =>
        {
            entity.ToTable("prices");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.AssetId).HasColumnName("assetid").IsRequired();
            entity.Property(e => e.Date).HasColumnName("date").IsRequired();
            entity.Property(e => e.Open).HasColumnName("open").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.High).HasColumnName("high").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.Low).HasColumnName("low").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.Close).HasColumnName("close").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.AdjustedClose).HasColumnName("adjustedclose").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.Volume).HasColumnName("volume").HasColumnType("BIGINT");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            
            // Optimize for PostgreSQL with proper indexing
            entity.HasIndex(e => new { e.AssetId, e.Date }).IsUnique().HasDatabaseName("ix_prices_assetid_date");
            entity.HasIndex(e => e.Date).HasDatabaseName("ix_prices_date");
            entity.HasIndex(e => e.AssetId).HasDatabaseName("ix_prices_assetid");
        });

        // Configure Portfolio entity with lowercase table and column names
        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.ToTable("portfolios");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Strategy).HasColumnName("strategy").HasMaxLength(50);
            entity.Property(e => e.TargetReturn).HasColumnName("targetreturn").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.MaxRisk).HasColumnName("maxrisk").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            entity.Property(e => e.IsActive).HasColumnName("isactive");
            
            // Add indexes for common queries
            entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_portfolios_isactive");
            entity.HasIndex(e => e.Strategy).HasDatabaseName("ix_portfolios_strategy");
        });

        // Configure PortfolioHolding entity with lowercase table and column names
        modelBuilder.Entity<PortfolioHolding>(entity =>
        {
            entity.ToTable("portfolioholdings");
            entity.Property(e => e.PortfolioId).HasColumnName("portfolioid").IsRequired();
            entity.Property(e => e.AssetId).HasColumnName("assetid").IsRequired();
            entity.Property(e => e.Weight).HasColumnName("weight").HasColumnType("NUMERIC(18,6)").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.AverageCost).HasColumnName("averagecost").HasColumnType("NUMERIC(18,6)");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            
            // Configure composite primary key
            entity.HasKey(e => new { e.PortfolioId, e.AssetId });
            
            // Configure relationships with proper cascade behavior
            entity.HasOne(e => e.Portfolio)
                  .WithMany(p => p.PortfolioHoldings)
                  .HasForeignKey(e => e.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.PortfolioHoldings)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Add indexes for performance
            entity.HasIndex(e => e.AssetId).HasDatabaseName("ix_portfolioholdings_assetid");
        });

        // Configure Asset relationships
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasMany(e => e.Prices)
                  .WithOne(p => p.Asset)
                  .HasForeignKey(p => p.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging only in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
