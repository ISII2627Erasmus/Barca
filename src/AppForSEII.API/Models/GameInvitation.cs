using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[PrimaryKey(nameof(TeamId), nameof(GameId))]
public class GameInvitation
{
    public int TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = null!;

    public int GameId { get; set; }

    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; } = null!;

    public bool AcceptedGame { get; set; } = false;

    [Required]
    [MinLength(11)]
    public string Message { get; set; } = string.Empty;
}