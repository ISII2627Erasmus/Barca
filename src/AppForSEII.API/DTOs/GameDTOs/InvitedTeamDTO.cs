namespace AppForSEII.API.DTOs.GameDTOs;

public class InvitedTeamDTO
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string CaptainName { get; set; } = string.Empty;

    public string CaptainSurname { get; set; } = string.Empty;

    public int NumberOfMembers { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public string SportName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}