using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace power4you_admin.Models;

[Table("Solarmodul")]
public class Solarmodul
{
    [Key]
    [Column("Modulnummer")]
    public int Modulnummer { get; set; }

    [Required]
    [Column("Solarmodultypnummer")]
    public int Solarmodultypnummer { get; set; }

    [Required]
    [Column("Kundennummer")]
    public int Kundennummer { get; set; }

    // Navigation properties
    [ForeignKey("Solarmodultypnummer")]
    public virtual Solarmodultyp Solarmodultyp { get; set; } = null!;

    [ForeignKey("Kundennummer")]
    public virtual Kunde Kunde { get; set; } = null!;

    public virtual ICollection<Leistung> Leistungen { get; set; } = new List<Leistung>();
} 