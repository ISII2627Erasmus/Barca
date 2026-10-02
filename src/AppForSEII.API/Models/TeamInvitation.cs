using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[PrimaryKey(nameof(UserId), nameof(TeamId))]
public class TeamInvitation
{
    public TeamInvitation()
    {
    }

    public TeamInvitation(
        string userId,
        int teamId,
        string invitationMessage)
    {
        UserId = userId;
        TeamId = teamId;
        InvitationMessage = invitationMessage;
        InvitationAccepted = false;
    }

    [Required]
    public string UserId { get; set; } = "0";

    [Required]
    public int TeamId { get; set; }

    [Required]
    [StringLength(
        255,
        ErrorMessage = "The message must be at least 3 characters long",
        MinimumLength = 3)]
    public string InvitationMessage { get; set; }
        = "Invitation Message for a team member";

    public bool InvitationAccepted { get; set; } = false;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = null!;
}
