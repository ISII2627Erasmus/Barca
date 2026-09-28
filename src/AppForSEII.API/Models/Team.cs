using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Team
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(9, ErrorMessage = "Team name must be between 4 and 9 characters.", MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MaximumNumberOfPlayers { get; set; }

    [Range(3, int.MaxValue)]
    public int MinimumAge { get; set; }

    [Range(3, int.MaxValue)]
    public int MaximumAge { get; set; }

    public string? Description { get; set; }

    [Required]
    public string CaptainId { get; set; } = string.Empty;

    [ForeignKey(nameof(CaptainId))]
    public ApplicationUser Captain { get; set; } = null!;

    [Required]
    public int SportId { get; set; }

}