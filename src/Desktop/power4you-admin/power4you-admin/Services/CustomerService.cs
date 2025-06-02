using Microsoft.EntityFrameworkCore;
using power4you_admin.Data;
using power4you_admin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace power4you_admin.Services;

public class CustomerService
{
    private readonly IDbContextFactory<SolarDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<SolarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Kunde>> GetAllKundenAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kunden.ToListAsync();
    }

    public async Task<Kunde?> GetKundeByIdAsync(int kundennummer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kunden.FindAsync(kundennummer);
    }
} 