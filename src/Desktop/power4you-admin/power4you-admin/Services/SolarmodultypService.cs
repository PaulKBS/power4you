using Microsoft.EntityFrameworkCore;
using power4you_admin.Data;
using power4you_admin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Required for FirstOrDefaultAsync

namespace power4you_admin.Services;

public class SolarmodultypService
{
    private readonly IDbContextFactory<SolarDbContext> _contextFactory;

    public SolarmodultypService(IDbContextFactory<SolarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Solarmodultyp>> GetAllSolarmodultypenAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodultypen.OrderBy(s => s.Bezeichnung).ToListAsync();
    }

    public async Task<Solarmodultyp?> GetSolarmodultypByIdAsync(int solarmodultypnummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Solarmodultypen.FindAsync(solarmodultypnummer);
    }

    public async Task<Solarmodultyp> AddSolarmodultypAsync(Solarmodultyp newTyp)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Solarmodultypen.Add(newTyp);
        await context.SaveChangesAsync();
        return newTyp; // newTyp will have the Solarmodultypnummer auto-generated if identity is set up.
    }

    public async Task<bool> UpdateSolarmodultypAsync(Solarmodultyp typToUpdate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existingTyp = await context.Solarmodultypen.FindAsync(typToUpdate.Solarmodultypnummer);
        if (existingTyp == null)
        {
            return false; // Not found
        }

        existingTyp.Bezeichnung = typToUpdate.Bezeichnung;
        existingTyp.Umpp = typToUpdate.Umpp;
        existingTyp.Impp = typToUpdate.Impp;
        existingTyp.Pmpp = typToUpdate.Pmpp;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSolarmodultypAsync(int solarmodultypnummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var typToDelete = await context.Solarmodultypen.FindAsync(solarmodultypnummer);
        if (typToDelete == null)
        {
            return false; // Not found
        }

        // Check for dependencies (Solarmodule using this type)
        bool isInUse = await context.Solarmodule.AnyAsync(s => s.Solarmodultypnummer == solarmodultypnummer);
        if (isInUse)
        {
            // Optionally, throw a specific exception or return a custom result indicating it's in use.
            // For now, let's prevent deletion if in use.
            throw new InvalidOperationException("Dieser Solarmodultyp wird von mindestens einem Solarmodul verwendet und kann nicht gelöscht werden.");
        }

        context.Solarmodultypen.Remove(typToDelete);
        await context.SaveChangesAsync();
        return true;
    }
} 