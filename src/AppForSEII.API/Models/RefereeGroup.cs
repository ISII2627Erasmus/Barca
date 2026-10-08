using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class RefereeGroup
{
    public RefereeGroup()
    {
    }

    public RefereeGroup(
        Game game,
        string name,
        string? description,
        string? rules)
    {
        GameId = game.Id;
        Game = game;
        Name = name;
        Description = description;
        Rules = rules;
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Rules { get; set; }

    [Required]
    public int GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; } = null!;

    public ICollection<RefereeAssignedTo> RefereeAssignments { get; set; }
        = new List<RefereeAssignedTo>();
}