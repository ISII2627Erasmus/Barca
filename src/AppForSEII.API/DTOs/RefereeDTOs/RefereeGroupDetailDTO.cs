namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereeGroupDetailDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Rules { get; set; }

    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public DateTime GameDate { get; set; }
    public string GamePlace { get; set; } = string.Empty;
    public string SportName { get; set; } = string.Empty;

    public List<RefereeAssignmentDetailDTO> RefereeAssignments { get; set; }
        = new();
}

public class RefereeAssignmentDetailDTO
{
    public string RefereeId { get; set; } = string.Empty;
    public string? RefereeName { get; set; }
    public string? RefereeSurname { get; set; }
    public int Rating { get; set; }
    public int YearsRefereeing { get; set; }
    public string Role { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public bool AcceptedAssignment { get; set; }
}