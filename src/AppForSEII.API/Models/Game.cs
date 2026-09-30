using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Game
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MinLength(6)]
    public string Place { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int SportId { get; set; }
    
    public Sport Sport { get; set; } = null!;

    public IList<GameInvitation> GameInvitations { get; set; } = new List<GameInvitation>();


    [Required]
    public string ResponsibleId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(ResponsibleId))]
    public ApplicationUser Responsible { get; set; } = null!;


}

