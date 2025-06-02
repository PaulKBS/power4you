using Microsoft.EntityFrameworkCore;
using power4you_admin.Data;
using power4you_admin.Models;

namespace power4you_admin.Services;

public class DatabaseService
{
    private readonly IDbContextFactory<SolarDbContext> _contextFactory;

    public DatabaseService(IDbContextFactory<SolarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
    public async Task<User> CreateUserAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.FindAsync(userId);
    }

    public async Task UpdateUserAsync(User user)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(userId);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    // Customer CRUD operations
    public async Task<Kunde> CreateKundeAsync(Kunde kunde)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Kunden.Add(kunde);
        await context.SaveChangesAsync();
        return kunde;
    }

    public async Task<List<Kunde>> GetAllKundenAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kunden
            .Include(k => k.User)
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Solarmodultyp)
            .ToListAsync();
    }

    public async Task<Kunde?> GetKundeByIdAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kunden
            .Include(k => k.User)
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Solarmodultyp)
            .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);
    }

    public async Task UpdateKundeAsync(Kunde kunde)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Kunden.Update(kunde);
        await context.SaveChangesAsync();
    }

    public async Task DeleteKundeAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden
            .Include(k => k.User)
            .Include(k => k.Solarmodule)
            .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);
        
        if (kunde != null)
        {
            if (kunde.Solarmodule.Any())
            {
                throw new InvalidOperationException("Cannot delete customer with existing solar modules. Please remove all solar modules first.");
            }

            context.Kunden.Remove(kunde);
            if (kunde.User != null)
            {
                context.Users.Remove(kunde.User);
            }
            await context.SaveChangesAsync();
        }
    }

    // Solar module methods
    public async Task<List<Solarmodul>> GetAllSolarmoduleAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodule
            .Include(s => s.Kunde)
            .Include(s => s.Solarmodultyp)
            .Include(s => s.Leistungen)
            .ToListAsync();
    }

    public async Task<List<Solarmodultyp>> GetAllSolarmodultypenAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodultypen.ToListAsync();
    }

    // Performance data methods
    public async Task<List<Leistung>> GetLeistungenByModulAsync(int modulnummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Leistungen
            .Where(l => l.Modulnummer == modulnummer)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Leistung>> GetRecentLeistungenAsync(int count = 100)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Leistungen
            .Include(l => l.Solarmodul)
                .ThenInclude(s => s.Kunde)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    // Statistics methods
    public async Task<int> GetTotalKundenCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kunden.CountAsync();
    }

    public async Task<int> GetTotalSolarmoduleCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodule.CountAsync();
    }

    public async Task<double> GetAveragePowerOutputAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var recentMeasurements = await context.Leistungen
            .Where(l => l.Timestamp >= DateTime.Now.AddDays(-1))
            .Select(l => l.PowerOut)
            .ToListAsync();

        return recentMeasurements.Any() ? recentMeasurements.Average() : 0;
    }

    public async Task<double> GetTotalPowerOutputAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var recentReadings = await context.Leistungen
            .Where(l => l.Timestamp >= DateTime.Now.AddHours(-1))
            .Select(l => new { l.Modulnummer, l.PowerOut, l.Timestamp })
            .ToListAsync();

        var latestPerModule = recentReadings
            .GroupBy(l => l.Modulnummer)
            .Select(g => g.OrderByDescending(l => l.Timestamp).First().PowerOut)
            .ToList();

        return latestPerModule.Sum();
    }

    // Enhanced analytics methods
    public async Task<double> GetTotalCapacityKWpAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodule
            .Include(s => s.Solarmodultyp)
            .SumAsync(s => s.Solarmodultyp.Pmpp) / 1000.0;
    }

    public async Task<double> GetPeakPowerTodayAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        var todayMeasurements = await context.Leistungen
            .Where(l => l.Timestamp >= today && l.Timestamp < tomorrow)
            .Select(l => l.PowerOut)
            .ToListAsync();

        return todayMeasurements.Any() ? todayMeasurements.Max() : 0;
    }

    public async Task<double> GetCurrentTotalOutputAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var recentMeasurements = await context.Leistungen
            .Where(l => l.Timestamp >= DateTime.Now.AddMinutes(-30)) // Last 30 minutes
            .Select(l => new { l.Modulnummer, l.PowerOut, l.Timestamp })
            .ToListAsync();

        var latestPerModule = recentMeasurements
            .GroupBy(l => l.Modulnummer)
            .Select(g => g.OrderByDescending(l => l.Timestamp).First().PowerOut)
            .ToList();

        return latestPerModule.Sum();
    }

    public async Task<List<(string ModuleType, int Count)>> GetModuleTypeDistributionAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Simplified query to avoid projection issues
        var moduleTypes = await context.Solarmodule
            .Include(s => s.Solarmodultyp)
            .ToListAsync();
        
        return moduleTypes
            .Where(s => s.Solarmodultyp != null)
            .GroupBy(s => s.Solarmodultyp.Bezeichnung)
            .Select(g => (g.Key, g.Count()))
            .ToList();
    }

    public async Task<List<(int ModuleNumber, double PowerOutput, string CustomerName)>> GetTopPerformingModulesAsync(int count = 3)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var measurements = await context.Leistungen
            .Where(l => l.Timestamp >= DateTime.Now.AddDays(-7)) // Last 7 days
            .Select(l => new 
            {
                l.Modulnummer,
                l.PowerOut
            })
            .ToListAsync();

        if (!measurements.Any())
            return new List<(int ModuleNumber, double PowerOutput, string CustomerName)>();

        var topPerformersData = measurements
            .GroupBy(l => l.Modulnummer)
            .Select(g => new 
            {
                ModuleNumber = g.Key,
                AvgPowerOutput = g.Average(l => l.PowerOut)
            })
            .OrderByDescending(x => x.AvgPowerOutput)
            .Take(count)
            .ToList();

        var results = new List<(int ModuleNumber, double PowerOutput, string CustomerName)>();
        foreach (var perfData in topPerformersData)
        {
            var module = await context.Solarmodule
                .Include(s => s.Kunde)
                .FirstOrDefaultAsync(s => s.Modulnummer == perfData.ModuleNumber);
            
            var customerName = module?.Kunde?.Nachname ?? "Unknown";
            results.Add((perfData.ModuleNumber, perfData.AvgPowerOutput, customerName));
        }
        return results;
    }

    public async Task<List<(DateTime Date, double AvgPower)>> GetPowerTrendDataAsync(int days = 7)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var startDate = DateTime.Now.AddDays(-days).Date;
        
        // Fetch data and group on client side to avoid EF translation issues
        var measurements = await context.Leistungen
            .Where(l => l.Timestamp >= startDate)
            .Select(l => new { l.Timestamp, l.PowerOut, l.Modulnummer })
            .ToListAsync();
        
        // Group by date and hour, then sum power across all modules for each time point
        return measurements
            .GroupBy(l => l.Timestamp.Date)
            .Select(dayGroup => 
            {
                // For each day, calculate total system power by summing all modules
                var hourlyTotals = dayGroup
                    .GroupBy(h => h.Timestamp.Hour)
                    .Select(hourGroup => 
                        hourGroup.GroupBy(m => m.Modulnummer)
                                 .Sum(moduleGroup => moduleGroup.Average(r => r.PowerOut))
                    );
                
                var avgDailyTotal = hourlyTotals.Any() ? hourlyTotals.Average() : 0;
                return (dayGroup.Key, avgDailyTotal);
            })
            .OrderBy(x => x.Key)
            .ToList();
    }

    public async Task<int> GetActiveModulesCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var recentThreshold = DateTime.Now.AddHours(-24);
        
        return await context.Leistungen
            .Where(l => l.Timestamp >= recentThreshold)
            .Select(l => l.Modulnummer)
            .Distinct()
            .CountAsync();
    }

    public async Task<double> GetSystemEfficiencyAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Calculate efficiency as (actual average power / theoretical max power) * 100
        var actualAvgPower = await GetAveragePowerOutputAsync();
        var totalCapacity = await GetTotalCapacityKWpAsync() * 1000; // Convert to watts
        
        if (totalCapacity == 0) return 0;
        
        // Assume theoretical efficiency under standard conditions (around 20% of peak capacity)
        var theoreticalPower = totalCapacity * 0.2;
        
        return Math.Min((actualAvgPower / theoreticalPower) * 100, 100);
    }
} 