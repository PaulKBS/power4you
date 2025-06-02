using System.ComponentModel.DataAnnotations;

namespace power4you_admin.Models;

/// <summary>
/// Anlage represents a solar installation for a customer.
/// This is an abstraction layer that groups all solar modules belonging to a customer.
/// </summary>
public class Anlage
{
    [Key]
    public int AnlageId { get; set; }
    
    [Required]
    public int Kundennummer { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Beschreibung { get; set; }
    
    public DateTime Erstellungsdatum { get; set; } = DateTime.Now;
    
    public DateTime? Installationsdatum { get; set; }
    
    [StringLength(255)]
    public string? Standort { get; set; }
    
    // Navigation properties
    public virtual Kunde Kunde { get; set; } = null!;
    public virtual ICollection<Solarmodul> Solarmodule { get; set; } = new List<Solarmodul>();
    
    // Calculated properties
    public int AnzahlModule => Solarmodule?.Count ?? 0;
    
    public double GesamtleistungKWp => Solarmodule?.Sum(m => m.Solarmodultyp?.Pmpp ?? 0) / 1000.0 ?? 0;
    
    public string FormattedName => $"Anlage {Kunde?.Nachname ?? "Unknown"}";
    
    public double? LetztePowerAusgabe => Solarmodule?
        .SelectMany(m => m.Leistungen)
        .OrderByDescending(l => l.Timestamp)
        .FirstOrDefault()?.PowerOut;
} 