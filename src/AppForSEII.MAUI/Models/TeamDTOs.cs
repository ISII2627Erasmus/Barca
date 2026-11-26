namespace AppForSEII.MAUI.Models;

public class InterestedInDTO
{
    public string SportName { get; set; } = string.Empty;
    public int Skill { get; set; }
}

public class ApplicationUserForTeamDTO
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int Age { get; set; }
    public List<InterestedInDTO> Sports { get; set; } = new();

    public string FullName => $"{Name} {Surname}".Trim();
}

public class TeamInvitationDTO
{
    public string UserId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int Age { get; set; }
    public string InvitationMessage { get; set; } = string.Empty;
}

public class TeamForCreateDTO
{
    public string Name { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public string SportName { get; set; } = string.Empty;
    public string CaptainUserName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TeamInvitationDTO> TeamInvitations { get; set; } = new();
}

public class TeamDetailDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportName { get; set; } = string.Empty;
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public string? CaptainName { get; set; }
    public string? CaptainSurname { get; set; }
    public int CaptainAge { get; set; }
    public List<TeamInvitationDTO> Invitations { get; set; } = new();
}


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