using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.GameDTOs;

public class GameForCreateDTO
{
    public GameForCreateDTO()
    {
    }

    public GameForCreateDTO(
        string name,
        DateTime date,
        string place,
        string? description,
        IList<GameInvitationDTO> gameInvitations)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Date = date;
        Place = place ?? throw new ArgumentNullException(nameof(place));
        Description = description;
        GameInvitations = gameInvitations
            ?? throw new ArgumentNullException(nameof(gameInvitations));
    }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [MinLength(6)]
    public string Place { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MinLength(1)]
    public IList<GameInvitationDTO> GameInvitations { get; set; }
        = new List<GameInvitationDTO>();

    public override bool Equals(object? obj)
    {
        return obj is GameForCreateDTO other &&
               Name == other.Name &&
               Date == other.Date &&
               Place == other.Place &&
               Description == other.Description &&
               GameInvitations.SequenceEqual(other.GameInvitations);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Date, Place, Description);
    }
}
