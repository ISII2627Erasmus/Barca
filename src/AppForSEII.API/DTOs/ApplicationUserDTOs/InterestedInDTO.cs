namespace AppForSEII.API.DTOs.ApplicationUserDTO;

public class InterestedInDTO
{
    public InterestedInDTO()
    {
    }

    public InterestedInDTO(string sportName, int skill)
    {
        SportName = sportName;
        Skill = skill;
    }

    [StringLength(50)]
    public string SportName { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Skill { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is InterestedInDTO dTO &&
               SportName == dTO.SportName &&
               Skill == dTO.Skill;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SportName, Skill);
    }
}