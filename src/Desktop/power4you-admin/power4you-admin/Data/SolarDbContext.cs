using Microsoft.EntityFrameworkCore;
using power4you_admin.Models;

namespace power4you_admin.Data;

public class SolarDbContext : DbContext
{
    public SolarDbContext(DbContextOptions<SolarDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Kunde> Kunden { get; set; } = null!;
    public DbSet<Solarmodultyp> Solarmodultypen { get; set; } = null!;
    public DbSet<Solarmodul> Solarmodule { get; set; } = null!;
    public DbSet<Leistung> Leistungen { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure Kunde entity
        modelBuilder.Entity<Kunde>(entity =>
        {
            entity.HasKey(e => e.Kundennummer);
            entity.HasIndex(e => e.UserId).IsUnique();
            
            entity.HasOne(k => k.User)
                  .WithOne(u => u.Kunde)
                  .HasForeignKey<Kunde>(k => k.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Solarmodultyp entity
        modelBuilder.Entity<Solarmodultyp>(entity =>
        {
            entity.HasKey(e => e.Solarmodultypnummer);
        });

        // Configure Solarmodul entity
        modelBuilder.Entity<Solarmodul>(entity =>
        {
            entity.HasKey(e => e.Modulnummer);
            
            entity.HasOne(s => s.Kunde)
                  .WithMany(k => k.Solarmodule)
                  .HasForeignKey(s => s.Kundennummer)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(s => s.Solarmodultyp)
                  .WithMany(st => st.Solarmodule)
                  .HasForeignKey(s => s.Solarmodultypnummer)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Leistung entity
        modelBuilder.Entity<Leistung>(entity =>
        {
            entity.HasKey(e => new { e.Timestamp, e.Modulnummer });
            
            entity.HasOne(l => l.Solarmodul)
                  .WithMany(s => s.Leistungen)
                  .HasForeignKey(l => l.Modulnummer)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 