using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[PrimaryKey(nameof(UserId), nameof(SportId))]
public class InterestedIn
{
    public InterestedIn()
    {
    }

    public InterestedIn(string userId, int sportId, int skill)
    {
        UserId = userId;
        SportId = sportId;
        Skill = skill;
    }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int SportId { get; set; }

    [Range(1, 5)]
    public int Skill { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser UserInterested { get; set; } = null!;

    [ForeignKey(nameof(SportId))]
    public Sport Sport { get; set; } = null!;
}