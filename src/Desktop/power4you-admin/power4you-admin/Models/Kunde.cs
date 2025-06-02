using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace power4you_admin.Models;

[Table("Kunde")]
public class Kunde
{
    [Key]
    [Column("Kundennummer")]
    public int Kundennummer { get; set; }

    [Required]
    [Column("User_ID")]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("Vorname")]
    public string Vorname { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Nachname")]
    public string Nachname { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Strasse")]
    public string Strasse { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Hausnummer")]
    public string Hausnummer { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Postleitzahl")]
    public string Postleitzahl { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Ort")]
    public string Ort { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Telefonnummer")]
    public string Telefonnummer { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<Solarmodul> Solarmodule { get; set; } = new List<Solarmodul>();

    [NotMapped]
    public bool HasAnlage { get; set; }

    [NotMapped]
    public string VollerName => $"{Vorname} {Nachname}";

    [NotMapped]
    public string VollständigeAdresse => $"{Strasse} {Hausnummer}, {Postleitzahl} {Ort}";
    
    [NotMapped]
    public string DisplayFullNameWithId => $"({Kundennummer}) {Vorname} {Nachname}";
} 