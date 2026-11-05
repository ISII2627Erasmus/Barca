
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Game
{
    public Game()
    {
    }

    public Game(
        string name,
        DateTime date,
        string place,
        string? description,
        Sport sport,
        ApplicationUser responsible)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Date = date;
        Place = place ?? throw new ArgumentNullException(nameof(place));
        Description = description;
        Sport = sport ?? throw new ArgumentNullException(nameof(sport));
        Responsible = responsible ?? throw new ArgumentNullException(nameof(responsible));
        ResponsibleId = responsible.Id;
    }

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

    public IList<GameInvitation> GameInvitations { get; set; }
        = new List<GameInvitation>();

    public RefereeGroup? RefereeGroup { get; set; }

    [Required]
    public string ResponsibleId { get; set; } = string.Empty;

    [ForeignKey(nameof(ResponsibleId))]
    public ApplicationUser Responsible { get; set; } = null!;
}
