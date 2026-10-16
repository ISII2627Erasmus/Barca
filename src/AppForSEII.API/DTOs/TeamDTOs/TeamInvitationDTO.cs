namespace AppForSEII.API.DTOs.TeamDTOs;

public class TeamInvitationDTO
{
    public TeamInvitationDTO()
    {
    }

    public TeamInvitationDTO(string userId, string? name, string? surname, int age, string invitationMessage)
    {
        UserId = userId;
        Name = name;
        Surname = surname;
        Age = age;
        InvitationMessage = invitationMessage;
    }

    public string UserId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Surname { get; set; }

    public int Age { get; set; }

    [StringLength(255, MinimumLength = 3)]
    public string InvitationMessage { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return obj is TeamInvitationDTO dTO &&
               UserId == dTO.UserId &&
               Name == dTO.Name &&
               Surname == dTO.Surname &&
               Age == dTO.Age &&
               InvitationMessage == dTO.InvitationMessage;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserId, Name, Surname, Age, InvitationMessage);
    }
}