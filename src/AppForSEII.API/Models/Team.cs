using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Team : IValidatableObject
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
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxMembers { get; set; }

    [Range(3, int.MaxValue)]
    public int MinAge { get; set; } = 3;

    [Range(3, int.MaxValue)]
    public int MaxAge { get; set; } = 3;

    public ApplicationUser Captain { get; set; } = null!;

    public Sport Sport { get; set; } = null!;

    public ICollection<TeamInvitation> TeamInvitations { get; set; }
        = new List<TeamInvitation>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaxAge < MinAge)
        {
            yield return new ValidationResult(
                "Maximum age must be greater than or equal to minimum age.",
                new[] { nameof(MinAge), nameof(MaxAge) });
        }
    }
}
