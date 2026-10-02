using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[PrimaryKey(nameof(RefereeGroupId), nameof(RefereeId))]
public class RefereeAssignedTo
{
    public RefereeAssignedTo()
    {
    }

    public RefereeAssignedTo(
        int refereeGroupId,
        string refereeId,
        string role,
        string roleDescription)
    {
        RefereeGroupId = refereeGroupId;
        RefereeId = refereeId;
        Role = role;
        RoleDescription = roleDescription;
        AcceptedAssignment = false;
    }

    [Required]
    public int RefereeGroupId { get; set; }

    [ForeignKey(nameof(RefereeGroupId))]
    public RefereeGroup RefereeGroup { get; set; } = null!;

    [Required]
    public string RefereeId { get; set; } = string.Empty;

    [ForeignKey(nameof(RefereeId))]
    public Referee Referee { get; set; } = null!;

    public bool AcceptedAssignment { get; set; } = false;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string RoleDescription { get; set; } = string.Empty;
}