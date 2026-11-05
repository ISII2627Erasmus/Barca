namespace AppForSEII.API.DTOs.GameDTOs;

public class GetGameDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Place { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string SportName { get; set; } = string.Empty;

    public List<InvitedTeamDTO> InvitedTeams { get; set; } = new();

    public override bool Equals(object? obj)
    {
        return obj is GetGameDTO other &&
               Id == other.Id &&
               Name == other.Name &&
               Date == other.Date &&
               Place == other.Place &&
               Description == other.Description &&
               SportName == other.SportName &&
               InvitedTeams.SequenceEqual(other.InvitedTeams);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Id, Name, Date, Place, Description, SportName);
    }
}