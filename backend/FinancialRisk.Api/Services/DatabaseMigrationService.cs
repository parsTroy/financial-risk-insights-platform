using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinancialRisk.Api.Data;

namespace FinancialRisk.Api.Services;

public interface IDatabaseMigrationService
{
    Task MigrateAsync();
    Task<bool> IsDatabaseUpToDateAsync();
}

public class DatabaseMigrationService : IDatabaseMigrationService
{
    private readonly FinancialRiskDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(FinancialRiskDbContext context, ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");
            
            // Ensure database exists
            await _context.Database.EnsureCreatedAsync();
            
            // Check if there are pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migration completed successfully");
            }
            else
            {
                _logger.LogInformation("Database is already up to date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database migration");
            throw;
        }
    }

    public async Task<bool> IsDatabaseUpToDateAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return !pendingMigrations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking database migration status");
            return false;
        }
    }
}
