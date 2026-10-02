using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PlantUmlClassDiagramGenerator.Attributes;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Team
{
    public Team()
    {
    }

    public Team(
        string name,
        int maxMembers,
        int minAge,
        int maxAge,
        ApplicationUser captain,
        Sport sport,
        string? description = null)
    {
        Name = name;
        MaxMembers = maxMembers;
        MinAge = minAge;
        MaxAge = maxAge;
        Captain = captain;
        Sport = sport;
        Description = description;
    }

    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(9, MinimumLength = 4,
        ErrorMessage = "Team name must be between 4 and 9 characters.")]
    public string Name { get; set; } = "Team name";

    public string? Description { get; set; } = "Team name";

    [Range(1, int.MaxValue)]
    public int MaxMembers { get; set; }

    [Range(3, int.MaxValue)]
    public int MinAge { get; set; } = 3;

    [Range(3, int.MaxValue)]
    public int MaxAge { get; set; } = 3;

    [PlantUmlIgnore]
    public ApplicationUser Captain { get; set; } = null!;

    [PlantUmlIgnore]
    public Sport Sport { get; set; } = null!;

}
