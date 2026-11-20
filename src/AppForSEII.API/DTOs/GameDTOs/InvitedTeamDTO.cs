
namespace AppForSEII.API.DTOs.GameDTOs;

public class InvitedTeamDTO
{
    public InvitedTeamDTO(
        int teamId,
        string teamName,
        string captainName,
        string captainSurname,
        int numberOfMembers,
        int minAge,
        int maxAge,
        string sportName,
        string message)
    {
        TeamId = teamId;
        TeamName = teamName ?? throw new ArgumentNullException(nameof(teamName));
        CaptainName = captainName ?? throw new ArgumentNullException(nameof(captainName));
        CaptainSurname = captainSurname ?? throw new ArgumentNullException(nameof(captainSurname));
        NumberOfMembers = numberOfMembers;
        MinAge = minAge;
        MaxAge = maxAge;
        SportName = sportName ?? throw new ArgumentNullException(nameof(sportName));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public int TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string CaptainName { get; set; } = string.Empty;

    public string CaptainSurname { get; set; } = string.Empty;

    public int NumberOfMembers { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public string SportName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return obj is InvitedTeamDTO other &&
               TeamId == other.TeamId &&
               TeamName == other.TeamName &&
               CaptainName == other.CaptainName &&
               CaptainSurname == other.CaptainSurname &&
               NumberOfMembers == other.NumberOfMembers &&
               MinAge == other.MinAge &&
               MaxAge == other.MaxAge &&
               SportName == other.SportName &&
               Message == other.Message;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            TeamId,
            TeamName,
            CaptainName,
            CaptainSurname,
            NumberOfMembers,
            MinAge,
            MaxAge,
            HashCode.Combine(SportName, Message));
    }
}
