namespace AppForSEII.API.DTOs.TeamDTO;

public class TeamForGameDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CaptainName { get; set; } = string.Empty;
    public string CaptainSurname { get; set; } = string.Empty;
    public int NumberOfMembers { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public string SportName { get; set; } = string.Empty;
}
