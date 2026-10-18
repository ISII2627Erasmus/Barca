namespace AppForSEII.API.DTOs.RefereeDTOs;

public class RefereeForRefereeGroupDTO
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int Rating { get; set; }
    public int YearsRefereeing { get; set; }
}