using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace power4you_admin.Models;

[Table("Leistung")]
public class Leistung
{
    [Key]
    [Column("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required]
    [Column("Modulnummer")]
    public int Modulnummer { get; set; }

    [Required]
    [Column("Power_Out")]
    public int PowerOut { get; set; }

    // Navigation property
    [ForeignKey("Modulnummer")]
    public virtual Solarmodul Solarmodul { get; set; } = null!;
} 