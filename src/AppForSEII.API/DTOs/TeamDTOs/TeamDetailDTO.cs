namespace AppForSEII.API.DTOs.TeamDTOs;

public class TeamDetailDTO
{
    public TeamDetailDTO()
    {
    }

    public TeamDetailDTO(int id, string name, string sportName, int minAge, int maxAge,
        string? captainName, string? captainSurname, int captainAge,
        IList<TeamInvitationDTO> invitations)
    {
        Id = id;
        Name = name;
        SportName = sportName;
        MinAge = minAge;
        MaxAge = maxAge;
        CaptainName = captainName;
        CaptainSurname = captainSurname;
        CaptainAge = captainAge;
        Invitations = invitations;
    }

    public int Id { get; set; }

    [StringLength(9, MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string SportName { get; set; } = string.Empty;

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    [StringLength(50)]
    public string? CaptainName { get; set; }

    [StringLength(50)]
    public string? CaptainSurname { get; set; }

    public int CaptainAge { get; set; }

    public IList<TeamInvitationDTO> Invitations { get; set; } = new List<TeamInvitationDTO>();

    public override bool Equals(object? obj)
    {
        return obj is TeamDetailDTO dTO &&
               Id == dTO.Id &&
               Name == dTO.Name &&
               SportName == dTO.SportName &&
               MinAge == dTO.MinAge &&
               MaxAge == dTO.MaxAge &&
               CaptainName == dTO.CaptainName &&
               CaptainSurname == dTO.CaptainSurname &&
               CaptainAge == dTO.CaptainAge &&
               Invitations.SequenceEqual(dTO.Invitations);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, SportName, MinAge, MaxAge, CaptainName, CaptainSurname, CaptainAge);
    }
}