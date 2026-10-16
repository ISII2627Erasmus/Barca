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
}