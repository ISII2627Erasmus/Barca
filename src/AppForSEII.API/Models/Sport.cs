using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Sport
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MinimumNumberOfPlayers { get; set; }
    [Range(1, int.MaxValue)]
    public int NumberOfReferees { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string BasicRules { get; set; } = string.Empty;
}