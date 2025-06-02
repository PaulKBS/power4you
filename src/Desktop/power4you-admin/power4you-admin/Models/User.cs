using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace power4you_admin.Models;

[Table("User")]
public class User
{
    [Key]
    [Column("User_ID")]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Api_key")]
    public string ApiKey { get; set; } = string.Empty;

    // Navigation property
    public virtual Kunde? Kunde { get; set; }
} 