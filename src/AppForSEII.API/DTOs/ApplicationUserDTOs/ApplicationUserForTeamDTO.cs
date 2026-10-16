namespace AppForSEII.API.DTOs.ApplicationUserDTO;

public class ApplicationUserForTeamDTO
{
    public ApplicationUserForTeamDTO()
    {
    }

    public ApplicationUserForTeamDTO(string id, string? name, string? surname, int age,
        IList<InterestedInDTO> sports)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Age = age;
        Sports = sports;
    }

    public string Id { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Surname { get; set; }

    public int Age { get; set; }

    public IList<InterestedInDTO> Sports { get; set; } = new List<InterestedInDTO>();

    public override bool Equals(object? obj)
    {
        return obj is ApplicationUserForTeamDTO dTO &&
               Id == dTO.Id &&
               Name == dTO.Name &&
               Surname == dTO.Surname &&
               Age == dTO.Age &&
               Sports.SequenceEqual(dTO.Sports);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Surname, Age);
    }
}