using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace power4you_admin.Models;

[Table("Solarmodultyp")]
public class Solarmodultyp
{
    [Key]
    [Column("Solarmodultypnummer")]
    public int Solarmodultypnummer { get; set; }

    [Required]
    [StringLength(64)]
    [Column("Bezeichnung")]
    public string Bezeichnung { get; set; } = string.Empty;

    [Required]
    [Column("Umpp")]
    public float Umpp { get; set; }

    [Required]
    [Column("Impp")]
    public float Impp { get; set; }

    [Required]
    [Column("Pmpp")]
    public float Pmpp { get; set; }

    // Navigation property
    public virtual ICollection<Solarmodul> Solarmodule { get; set; } = new List<Solarmodul>();
} 