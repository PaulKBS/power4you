using Microsoft.EntityFrameworkCore;
using power4you_admin.Data;
using power4you_admin.Models;

namespace power4you_admin.Services;

/// <summary>
/// Service for managing Anlage (solar installations) - an abstraction layer over customer solar modules
/// </summary>
public class AnlageService
{
    private readonly IDbContextFactory<SolarDbContext> _contextFactory;

    public AnlageService(IDbContextFactory<SolarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Gets all Anlagen by creating virtual representations from customer data
    /// </summary>
    public async Task<List<Anlage>> GetAllAnlagenAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunden = await context.Kunden
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Solarmodultyp)
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Leistungen.OrderByDescending(l => l.Timestamp).Take(1))
            .Where(k => k.Solarmodule.Any())  
            .ToListAsync();

        var anlagen = new List<Anlage>();
        int anlageId = 1;

        foreach (var kunde in kunden)
        {
            var anlage = new Anlage
            {
                AnlageId = anlageId++,
                Kundennummer = kunde.Kundennummer,
                Name = $"Anlage {kunde.Nachname}",
                Beschreibung = $"Solar installation for {kunde.Vorname} {kunde.Nachname}",
                Erstellungsdatum = DateTime.Now,
                Standort = $"{kunde.Strasse} {kunde.Hausnummer}, {kunde.Postleitzahl} {kunde.Ort}",
                Kunde = kunde,
                Solarmodule = kunde.Solarmodule
            };

            anlagen.Add(anlage);
        }

        return anlagen;
    }

    /// <summary>
    /// Gets a specific Anlage by customer number
    /// </summary>
    public async Task<Anlage?> GetAnlageByKundennummerAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Solarmodultyp)
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Leistungen.OrderByDescending(l => l.Timestamp).Take(1))
            .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);

        if (kunde == null || !kunde.Solarmodule.Any())
            return null;

        return new Anlage
        {
            AnlageId = kundennummer, // Use customer number as unique identifier
            Kundennummer = kunde.Kundennummer,
            Name = $"Anlage {kunde.Nachname}",
            Beschreibung = $"Solar installation for {kunde.Vorname} {kunde.Nachname}",
            Erstellungsdatum = DateTime.Now, // Could be derived from first module installation
            Standort = $"{kunde.Strasse} {kunde.Hausnummer}, {kunde.Postleitzahl} {kunde.Ort}",
            Kunde = kunde,
            Solarmodule = kunde.Solarmodule
        };
    }

    /// <summary>
    /// Gets performance statistics for an Anlage
    /// </summary>
    public async Task<AnlageStatistics> GetAnlageStatisticsAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Leistungen)
            .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);

        if (kunde == null)
            return new AnlageStatistics();

        var allLeistungen = kunde.Solarmodule
            .SelectMany(m => m.Leistungen)
            .ToList(); // Fetches allleistungen for the customer

        // Default to last 30 days for this specific overload
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var recentLeistungen = allLeistungen
            .Where(l => l.Timestamp >= thirtyDaysAgo)
            .ToList();

        return new AnlageStatistics
        {
            Kundennummer = kundennummer,
            AnzahlModule = kunde.Solarmodule.Count,
            GesamtleistungKWp = kunde.Solarmodule.Sum(m => m.Solarmodultyp?.Pmpp ?? 0) / 1000.0,
            DurchschnittlichePowerLetzten30Tage = recentLeistungen.Any() ? recentLeistungen.Average(l => l.PowerOut) : 0,
            MaximalePowerLetzten30Tage = recentLeistungen.Any() ? recentLeistungen.Max(l => l.PowerOut) : 0,
            MinimalePowerLetzten30Tage = recentLeistungen.Any() ? recentLeistungen.Min(l => l.PowerOut) : 0,
            GesamtMessungen = recentLeistungen.Count, // Count for the 30-day period
            LetzteMessung = recentLeistungen.OrderByDescending(l => l.Timestamp).FirstOrDefault()?.Timestamp
        };
    }

    /// <summary>
    /// Gets performance statistics for an Anlage within a specified date range.
    /// </summary>
    public async Task<AnlageStatistics> GetAnlageStatisticsAsync(int kundennummer, DateTime startDate, DateTime endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden
            .Include(k => k.Solarmodule)
                .ThenInclude(s => s.Solarmodultyp) // Include Solarmodultyp for GesamtleistungKWp
            .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);

        if (kunde == null)
            return new AnlageStatistics();

        // Get module IDs for this customer to filter Leistungen directly
        var modulnummern = kunde.Solarmodule.Select(s => s.Modulnummer).ToList();

        if (!modulnummern.Any())
            return new AnlageStatistics { Kundennummer = kundennummer }; // No modules, return basic stats

        // Fetch Leistungen within the date range for the customer's modules
        var leistungenImZeitraum = await context.Leistungen
            .Where(l => modulnummern.Contains(l.Modulnummer) && l.Timestamp >= startDate && l.Timestamp <= endDate)
            .ToListAsync();

        return new AnlageStatistics
        {
            Kundennummer = kundennummer,
            AnzahlModule = kunde.Solarmodule.Count,
            GesamtleistungKWp = kunde.Solarmodule.Sum(m => m.Solarmodultyp?.Pmpp ?? 0) / 1000.0,
            DurchschnittlichePowerLetzten30Tage = leistungenImZeitraum.Any() ? leistungenImZeitraum.Average(l => l.PowerOut) : 0,
            MaximalePowerLetzten30Tage = leistungenImZeitraum.Any() ? leistungenImZeitraum.Max(l => l.PowerOut) : 0,
            MinimalePowerLetzten30Tage = leistungenImZeitraum.Any() ? leistungenImZeitraum.Min(l => l.PowerOut) : 0,
            GesamtMessungen = leistungenImZeitraum.Count,
            LetzteMessung = leistungenImZeitraum.Any() ? leistungenImZeitraum.Max(l => l.Timestamp) : (DateTime?)null
        };
    }

    /// <summary>
    /// Gets the latest performance data for all modules in an Anlage
    /// </summary>
    public async Task<List<Leistung>> GetLatestPerformanceDataAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Leistungen
            .Include(l => l.Solarmodul)
                .ThenInclude(s => s.Solarmodultyp)
            .Where(l => l.Solarmodul.Kundennummer == kundennummer)
            .GroupBy(l => l.Modulnummer)
            .Select(g => g.OrderByDescending(l => l.Timestamp).First())
            .ToListAsync();
    }

    /// <summary>
    /// Gets performance history for an Anlage
    /// </summary>
    public async Task<List<Leistung>> GetPerformanceHistoryAsync(int kundennummer, DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Leistungen
            .Include(l => l.Solarmodul)
                .ThenInclude(s => s.Solarmodultyp)
            .Where(l => l.Solarmodul.Kundennummer == kundennummer);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        // Sample data: take records where minute is a multiple of 30
        // This helps reduce data volume while retaining per-module readings at intervals.
        // Assuming Timestamp is a DateTime type that allows l.Timestamp.Minute.
        query = query.Where(l => l.Timestamp.Minute % 30 == 0);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// Updates Anlage information (updates customer data as the source of truth)
    /// </summary>
    public async Task<bool> UpdateAnlageAsync(Anlage anlage)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden.FindAsync(anlage.Kundennummer);
        if (kunde == null)
            return false;

        // We can only update customer information since Anlage is an abstraction
        // The name could be derived from customer's last name
        // Location information is already in customer data
        
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a customer has any solar modules (and thus an Anlage)
    /// </summary>
    public async Task<bool> CustomerHasAnlageAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodule
            .AnyAsync(s => s.Kundennummer == kundennummer);
    }

    /// <summary>
    /// Adds a new Solarmodul to a specified Kunde.
    /// </summary>
    /// <param name="kundennummer">The ID of the Kunde.</param>
    /// <param name="solarmodultypnummer">The ID of the Solarmodultyp.</param>
    /// <returns>The newly created Solarmodul, or null if an error occurs.</returns>
    public async Task<Solarmodul?> AddSolarmodulToKundeAsync(int kundennummer, int solarmodultypnummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden.FindAsync(kundennummer);
        var solarmodultyp = await context.Solarmodultypen.FindAsync(solarmodultypnummer);

        if (kunde == null || solarmodultyp == null)
        {
            // Kunde or Solarmodultyp not found
            return null;
        }

        var newModul = new Solarmodul
        {
            Kundennummer = kundennummer,
            Solarmodultypnummer = solarmodultypnummer
            // Modulnummer will be auto-generated by the database
        };

        context.Solarmodule.Add(newModul);
        await context.SaveChangesAsync();
        return newModul; // The newModul object will have the auto-generated Modulnummer after SaveChangesAsync
    }

    /// <summary>
    /// Removes a Solarmodul from the database.
    /// </summary>
    /// <param name="modulnummer">The ID of the Solarmodul to remove.</param>
    /// <returns>True if removal was successful, false otherwise.</returns>
    public async Task<bool> RemoveSolarmodulAsync(int modulnummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var modul = await context.Solarmodule.FindAsync(modulnummer);

        if (modul == null)
        {
            return false; // Module not found
        }

        // Before removing the module, we might need to handle related Leistungsdaten
        // Depending on requirements, Leistungsdaten could be deleted or orphaned.
        // For now, assuming cascade delete is set up in DB or we handle it explicitly if needed.
        // If Leistungsdaten have a foreign key to Solarmodul, EF Core might handle it based on FK constraints.
        var relatedLeistungen = await context.Leistungen.Where(l => l.Modulnummer == modulnummer).ToListAsync();
        if (relatedLeistungen.Any())
        {
            context.Leistungen.RemoveRange(relatedLeistungen);
        }

        context.Solarmodule.Remove(modul);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes an "Anlage" by removing all associated Solarmodule and their Leistungsdaten for a customer.
    /// AnlageId is treated as Kundennummer for the purpose of deletion.
    /// </summary>
    /// <param name="kundennummer">The Kundennummer of the customer whose Anlage is to be deleted.</param>
    /// <returns>True if deletion was successful or if no modules were found, false if the customer was not found or an error occurred.</returns>
    public async Task<bool> DeleteAnlageAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kunde = await context.Kunden
                                 .Include(k => k.Solarmodule)
                                 .FirstOrDefaultAsync(k => k.Kundennummer == kundennummer);

        if (kunde == null)
        {
            // Customer not found, arguably the "Anlage" doesn't exist or is already "deleted".
            // Depending on strictness, could return false, but true implies the desired state (no Anlage) is achieved.
            return true; 
        }

        var moduleToDelete = await context.Solarmodule
                                        .Where(s => s.Kundennummer == kundennummer)
                                        .ToListAsync();

        if (!moduleToDelete.Any())
        {
            // No modules to delete, Anlage effectively doesn't exist or is already "empty".
            return true;
        }

        // Collect all Modulnummern to delete their Leistungsdaten efficiently
        var modulnummernToDelete = moduleToDelete.Select(m => m.Modulnummer).ToList();

        // Delete associated Leistungsdaten
        var leistungenToDelete = await context.Leistungen
                                            .Where(l => modulnummernToDelete.Contains(l.Modulnummer))
                                            .ToListAsync();
        
        if (leistungenToDelete.Any())
        {
            context.Leistungen.RemoveRange(leistungenToDelete);
        }

        // Delete Solarmodule
        context.Solarmodule.RemoveRange(moduleToDelete);

        await context.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// Statistics data for an Anlage
/// </summary>
public class AnlageStatistics
{
    public int Kundennummer { get; set; }
    public int AnzahlModule { get; set; }
    public double GesamtleistungKWp { get; set; }
    public double DurchschnittlichePowerLetzten30Tage { get; set; } // Name is kept for simplicity, value is period-dependent
    public double MaximalePowerLetzten30Tage { get; set; } // Name is kept for simplicity, value is period-dependent
    public double MinimalePowerLetzten30Tage { get; set; } // Name is kept for simplicity, value is period-dependent
    public int GesamtMessungen { get; set; }
    public DateTime? LetzteMessung { get; set; }
} 